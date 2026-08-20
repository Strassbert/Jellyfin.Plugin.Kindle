#!/usr/bin/env python3
"""Checks the UI string tables against what the frontends actually reference.

The three frontends used to carry their own copies of the same tables, which is how
they drifted apart. Now they share Localization/*.json - this guards the new failure
mode: a key referenced in JavaScript or HTML that no language file defines would
render as a raw key like "button.send" in the interface.

Also verifies that every translation has the same key set as the English base, so a
missing translation is caught here rather than silently falling back in production.
"""
from __future__ import annotations

import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
LOCALIZATION = ROOT / "Localization"
BASE_LANGUAGE = "en"

FRONTENDS = [
    ROOT / "Web" / "kindleButton.js",
    ROOT / "Configuration" / "configPage.html",
    ROOT / "Configuration" / "userSettings.html",
]

# t('some.key') in JavaScript and data-i18n="some.key" in markup.
CALL_PATTERN = re.compile(r"""\bt\(\s*['"]([a-z][A-Za-z0-9]*\.[A-Za-z0-9]+)['"]""")
ATTR_PATTERN = re.compile(r"""data-i18n\s*=\s*['"]([^'"]+)['"]""")
FALLBACK_PATTERN = re.compile(r"""localizedError\([^,]+,\s*['"]([a-z][A-Za-z0-9]*\.[A-Za-z0-9]+)['"]""")


def main() -> int:
    base_path = LOCALIZATION / f"{BASE_LANGUAGE}.json"
    base = json.loads(base_path.read_text(encoding="utf-8"))
    errors: list[str] = []

    for path in sorted(LOCALIZATION.glob("*.json")):
        code = path.stem
        if code == BASE_LANGUAGE:
            continue

        translation = json.loads(path.read_text(encoding="utf-8"))
        missing = sorted(set(base) - set(translation))
        extra = sorted(set(translation) - set(base))

        if missing:
            errors.append(f"{code}.json is missing {len(missing)} key(s): {', '.join(missing[:8])}")
        if extra:
            errors.append(f"{code}.json has {len(extra)} key(s) not in {BASE_LANGUAGE}.json: {', '.join(extra[:8])}")

    used: set[str] = set()
    for path in FRONTENDS:
        text = path.read_text(encoding="utf-8")
        found = set(CALL_PATTERN.findall(text)) | set(ATTR_PATTERN.findall(text)) | set(FALLBACK_PATTERN.findall(text))
        unknown = sorted(k for k in found if k not in base)
        if unknown:
            errors.append(f"{path.relative_to(ROOT)} references undefined key(s): {', '.join(unknown)}")
        used |= found

    unused = sorted(set(base) - used)
    if unused:
        # Not fatal: a key may be referenced in a way the patterns above do not see.
        print(f"NOTE: {len(unused)} key(s) appear unused: {', '.join(unused[:10])}")

    if errors:
        for error in errors:
            print(f"ERROR: {error}", file=sys.stderr)
        return 1

    print(f"String check OK: {len(base)} keys, {len(used)} referenced, "
          f"{len(list(LOCALIZATION.glob('*.json')))} language(s)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
