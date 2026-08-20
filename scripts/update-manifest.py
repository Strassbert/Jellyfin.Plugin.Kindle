#!/usr/bin/env python3
"""Upserts a version entry in manifest.json.

Run by the release workflow once the zip exists, so the published checksum always
belongs to the artifact that was actually uploaded. Hand-editing this file is what
let the 1.2.0.0 entry drift away from the assembly it pointed at.
"""
import argparse
import json
import pathlib
import sys


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--version", required=True)
    parser.add_argument("--checksum", required=True)
    parser.add_argument("--source-url", required=True)
    parser.add_argument("--changelog", default="")
    parser.add_argument("--target-abi", default="10.11.0.0")
    parser.add_argument("--timestamp", required=True)
    parser.add_argument(
        "--manifest",
        default=str(pathlib.Path(__file__).resolve().parent.parent / "manifest.json"),
    )
    args = parser.parse_args()

    path = pathlib.Path(args.manifest)
    data = json.loads(path.read_text(encoding="utf-8"))

    entry = {
        "version": args.version,
        "changelog": args.changelog,
        "targetAbi": args.target_abi,
        "sourceUrl": args.source_url,
        "checksum": args.checksum.upper(),
        "timestamp": args.timestamp,
    }

    versions = data[0].setdefault("versions", [])
    versions = [v for v in versions if v.get("version") != args.version]
    versions.insert(0, entry)
    data[0]["versions"] = versions

    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"manifest.json updated for {args.version} ({entry['checksum']})")
    return 0


if __name__ == "__main__":
    sys.exit(main())
