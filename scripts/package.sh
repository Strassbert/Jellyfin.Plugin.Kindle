#!/usr/bin/env bash
#
# Builds the release artifact: a zip containing the plugin assemblies plus a
# meta.json. Jellyfin generates meta.json itself when a plugin is installed from a
# repository, but not for a manual drop into the plugins folder - in that case it
# falls back to deriving an id from the folder name (PluginManager.LoadManifest),
# which never matches Plugin.Id and leaves the plugin unremovable. Shipping the
# file makes manual installs behave like catalogue installs.
#
# JELLYFIN_TARGET selects the runtime (jf10 = Jellyfin 10.11 / .NET 9, the default
# and the only one currently released; jf12 = Jellyfin 12 / .NET 10).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

TARGET="${JELLYFIN_TARGET:-jf10}"
CSPROJ="Jellyfin.Plugin.Kindle.csproj"

python3 "$ROOT/scripts/verify_version.py"
python3 "$ROOT/scripts/verify_strings.py"

# Read straight from MSBuild so the packaged ABI can never drift from the assembly
# that was actually compiled.
VERSION="$(dotnet msbuild "$CSPROJ" -getProperty:Version -p:JellyfinTarget="$TARGET" | tr -d '[:space:]')"
TARGET_ABI="$(dotnet msbuild "$CSPROJ" -getProperty:PluginTargetAbi -p:JellyfinTarget="$TARGET" | tr -d '[:space:]')"

OUT="$ROOT/artifacts"
STAGE="$OUT/stage"

rm -rf "$OUT"
mkdir -p "$STAGE"

dotnet publish "$CSPROJ" -c Release -o "$OUT/publish" -p:JellyfinTarget="$TARGET" --nologo

# Only the plugin and its non-Jellyfin dependencies. Everything from the
# Jellyfin.Controller/Model packages is PrivateAssets and already loaded by the host;
# shipping a second copy risks the assembly-conflict path that marks a plugin
# "NotSupported" on startup.
for dll in Jellyfin.Plugin.Kindle.dll MailKit.dll MimeKit.dll BouncyCastle.Cryptography.dll; do
    if [ ! -f "$OUT/publish/$dll" ]; then
        echo "ERROR: expected $dll in publish output" >&2
        exit 1
    fi
    cp "$OUT/publish/$dll" "$STAGE/"
done

cp "$OUT/publish/Jellyfin.Plugin.Kindle.deps.json" "$STAGE/"

python3 - "$STAGE/meta.json" "$VERSION" "$TARGET_ABI" <<'PY'
import json, sys, datetime

target, version, target_abi = sys.argv[1], sys.argv[2], sys.argv[3]

# PascalCase keys and a string Status: PluginManager reads meta.json with
# JsonDefaults.Options, which sets PropertyNamingPolicy = null and is case sensitive.
# "Assemblies": [] means "load every dll in the folder"; a populated list would be
# treated as a strict allow-list and a single mismatch marks the plugin Malfunctioned.
manifest = {
    "Category": "Email",
    "Changelog": "",
    "Description": "Send e-books (EPUB, PDF, MOBI, AZW3) directly from the detail page to an E-Book Reader via email.",
    "Id": "e3b2b4a1-1234-4567-89ab-cdef12345678",
    "Name": "E-Book Share",
    "Overview": "Adds a 'Send to reader' button on book detail pages.",
    "Owner": "Strassbert",
    "TargetAbi": target_abi,
    "Timestamp": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
    "Version": version,
    "Status": "Active",
    "AutoUpdate": True,
    "Assemblies": [],
}

with open(target, "w", encoding="utf-8") as handle:
    json.dump(manifest, handle, indent=2)
PY

if [ "$TARGET" = "jf10" ]; then
    ZIP="$OUT/Jellyfin.Plugin.Kindle_$VERSION.zip"
else
    ZIP="$OUT/Jellyfin.Plugin.Kindle_${VERSION}_${TARGET}.zip"
fi

(cd "$STAGE" && zip -q -r "$ZIP" .)

CHECKSUM="$(md5sum "$ZIP" | cut -d' ' -f1)"

echo "target=$TARGET"
echo "version=$VERSION"
echo "targetAbi=$TARGET_ABI"
echo "zip=$ZIP"
echo "checksum=$CHECKSUM"

# Consumed by the release workflow via $GITHUB_OUTPUT.
if [ -n "${GITHUB_OUTPUT:-}" ]; then
    {
        echo "version=$VERSION"
        echo "targetAbi=$TARGET_ABI"
        echo "zip=$ZIP"
        echo "checksum=$CHECKSUM"
    } >> "$GITHUB_OUTPUT"
fi
