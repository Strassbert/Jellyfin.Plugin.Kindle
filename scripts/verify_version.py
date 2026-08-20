#!/usr/bin/env python3
"""Keeps the plugin's identity consistent across csproj, plugin.cs and manifest.json.

This exists because of a concrete failure. Release 1.2.0.0 shipped an assembly whose
AssemblyVersion was still 1.1.0.0. Jellyfin's dashboard reads PluginInfo.Version from
the assembly (BasePlugin<T> passes assemblyName.Version to SetAttributes) but resolves
DELETE /Plugins/{id}/{version} against meta.json, which it generates from
manifest.json. The two disagreed, GetPlugin(id, version) matched nothing, and
uninstalling the plugin silently returned 404. Enable/disable and the catalogue image
broke for the same reason.

Checks:
  * <Version>, <AssemblyVersion> and <FileVersion> agree with each other
  * the version parses as a System.Version
  * manifest.json never advertises a version newer than the assembly builds
  * manifest.json guid == Plugin.Id
  * manifest.json name == Plugin.Name (Jellyfin rewrites meta.json from the running
    instance, so a mismatch makes the on-disk manifest drift)

Pass --strict to additionally require that manifest.json already publishes exactly the
version being built; used at release time.
"""
from __future__ import annotations

import argparse
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
CSPROJ = ROOT / "Jellyfin.Plugin.Kindle.csproj"
MANIFEST = ROOT / "manifest.json"
PLUGIN_CS = ROOT / "plugin.cs"

errors: list[str] = []


def fail(message: str) -> None:
    errors.append(message)


def read_tag(text: str, tag: str) -> str | None:
    match = re.search(rf"<{tag}>([^<]*)</{tag}>", text)
    return match.group(1).strip() if match else None


def parse_version(value: str) -> tuple[int, ...]:
    return tuple(int(part) for part in value.split("."))


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--strict", action="store_true")
    args = parser.parse_args()

    csproj = CSPROJ.read_text(encoding="utf-8")
    plugin_cs = PLUGIN_CS.read_text(encoding="utf-8")
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))[0]

    version = read_tag(csproj, "Version")
    if not version:
        print("ERROR: <Version> missing from the csproj", file=sys.stderr)
        return 1

    for tag in ("AssemblyVersion", "FileVersion"):
        value = read_tag(csproj, tag)
        if value != version:
            fail(f"<{tag}> is {value!r} but <Version> is {version!r}")

    if not re.fullmatch(r"\d+(\.\d+){1,3}", version):
        fail(f"version {version!r} is not a valid System.Version")

    versions = manifest.get("versions") or []
    if not versions:
        fail("manifest.json has no versions")
    else:
        newest = versions[0]["version"]
        if parse_version(newest) > parse_version(version):
            fail(
                f"manifest.json advertises {newest} but the assembly builds {version}. "
                f"Jellyfin would list the plugin as {version} while meta.json says "
                f"{newest}, so uninstall/enable/disable resolve to nothing and 404."
            )
        elif args.strict and newest != version:
            fail(f"--strict: manifest.json newest entry is {newest}, expected {version}")
        elif newest != version:
            print(f"NOTE: {version} is not published yet (manifest newest: {newest}).")

    guid_match = re.search(r'Guid\.Parse\("([^"]+)"\)', plugin_cs)
    plugin_guid = guid_match.group(1).lower() if guid_match else None
    manifest_guid = str(manifest.get("guid", "")).lower()
    if plugin_guid != manifest_guid:
        fail(f"manifest.json guid {manifest_guid!r} != Plugin.Id {plugin_guid!r}")

    name_match = re.search(r'PluginName\s*=\s*"([^"]+)"', plugin_cs)
    plugin_name = name_match.group(1) if name_match else None
    if plugin_name != manifest.get("name"):
        fail(
            f"manifest.json name {manifest.get('name')!r} != Plugin.Name {plugin_name!r}. "
            "Jellyfin rewrites meta.json from the running instance, so these must agree."
        )

    if errors:
        for error in errors:
            print(f"ERROR: {error}", file=sys.stderr)
        return 1

    print(f"Version check OK: {version} / {plugin_name} / {plugin_guid}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
