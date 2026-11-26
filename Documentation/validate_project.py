#!/usr/bin/env python3
import json
import pathlib
import re
import xml.etree.ElementTree as ET

root = pathlib.Path(__file__).resolve().parents[1]
required = [
    "Assets/Scripts/NooMiBetterRuntime.cs",
    "Assets/Scripts/Systems/PerformanceSettings.cs",
    "Assets/Scripts/Systems/ReplayRecorder.cs",
    "Assets/Editor/NooMiBetterProjectSetup.cs",
    "Assets/Editor/NooMiBuild.cs",
    "Packages/manifest.json",
    "ProjectSettings/ProjectVersion.txt",
    "Server/relay-server.js",
]
missing = [p for p in required if not (root / p).is_file()]
if missing:
    raise SystemExit("FAIL: missing required files: " + ", ".join(missing))

json.loads((root / "Packages/manifest.json").read_text(encoding="utf-8"))
for manifest in (root / "Assets").rglob("*.xml"):
    ET.parse(manifest)

runtime = (root / "Assets/Scripts/NooMiBetterRuntime.cs").read_text(encoding="utf-8")
assert "class NooMiBetterApp" in runtime
assert "RuntimeInitializeOnLoadMethod" in runtime
assert "class NooMiBetterBootstrap" in runtime
assert "com.noomibetter.game" in (root / "Assets/Editor/NooMiBetterProjectSetup.cs").read_text(encoding="utf-8")
assert "AndroidApiLevel29" in (root / "Assets/Editor/NooMiBetterProjectSetup.cs").read_text(encoding="utf-8")
assert "AndroidArchitecture.ARM64" in (root / "Assets/Editor/NooMiBetterProjectSetup.cs").read_text(encoding="utf-8")

# Basic delimiter sanity check after stripping comments and string literals.
for path in (root / "Assets").rglob("*.cs"):
    source = path.read_text(encoding="utf-8")
        source = re.sub(r'//[^\n]*|/\*.*?\*/|@?"(?:\\.|[^"\\])*"', "", source, flags=re.S)
    for left, right in (("{", "}"), ("(", ")"), ("[", "]")):
        depth = 0
        for char in source:
            if char == left: depth += 1
            elif char == right:
                depth -= 1
                if depth < 0: raise SystemExit(f"FAIL: unmatched {right} in {path}")
        if depth: raise SystemExit(f"FAIL: unmatched {left} in {path}")

print("PASS: project files, JSON/XML, Android settings and C# delimiters")
