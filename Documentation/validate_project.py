#!/usr/bin/env python3
import json
import pathlib
import xml.etree.ElementTree as ET

root = pathlib.Path(__file__).resolve().parents[1]
required = [
    "Assets/Scripts/NooMiBetterRuntime.cs",
    "Assets/Scripts/Physics/RagdollController.cs",
    "Assets/Scripts/Multiplayer/MultiplayerRelayClient.cs",
    "Assets/Scripts/Systems/PerformanceSettings.cs",
    "Assets/Scripts/Systems/ReplayRecorder.cs",
    "Assets/Scripts/Systems/CosmeticShop.cs",
    "Assets/Scripts/Systems/AchievementTracker.cs",
    "Assets/Editor/NooMiBetterProjectSetup.cs",
    "Assets/Editor/NooMiBuild.cs",
    "Packages/manifest.json",
    "ProjectSettings/ProjectVersion.txt",
    "Server/relay-server.js",
]
missing = [p for p in required if not (root / p).is_file()]
if missing:
    raise SystemExit("FAIL: missing required files: " + ", ".join(missing))

manifest = json.loads((root / "Packages/manifest.json").read_text(encoding="utf-8"))
for module in ("com.unity.modules.unitywebrequest", "com.unity.modules.unitywebrequestaudio"):
    if module not in manifest["dependencies"]:
        raise SystemExit("FAIL: missing Unity module " + module)
for xml_file in (root / "Assets").rglob("*.xml"):
    ET.parse(xml_file)

runtime = (root / "Assets/Scripts/NooMiBetterRuntime.cs").read_text(encoding="utf-8")
setup = (root / "Assets/Editor/NooMiBetterProjectSetup.cs").read_text(encoding="utf-8")
assert "class NooMiBetterApp" in runtime
assert "RagdollController" in runtime and "MultiplayerRelayClient" in runtime
assert "TexturePatternGenerator" in runtime and "ReplayRecorder" in runtime
assert "AndroidApiLevel29" in setup and "AndroidArchitecture.ARM64" in setup
assert "com.noomibetter.game" in setup

def strip_comments_and_strings(source):
    out = []
    i = 0
    state = "code"
    while i < len(source):
        ch = source[i]
        nxt = source[i + 1] if i + 1 < len(source) else ""
        if state == "code":
            if ch == "/" and nxt == "/":
                state = "line"; out.extend("  "); i += 2; continue
            if ch == "/" and nxt == "*":
                state = "block"; out.extend("  "); i += 2; continue
            if ch == '"':
                state = "string"; out.append(" "); i += 1; continue
            if ch == "'":
                state = "char"; out.append(" "); i += 1; continue
            out.append(ch); i += 1; continue
        if state == "line":
            if ch == "\n":
                state = "code"; out.append("\n")
            else:
                out.append(" ")
            i += 1; continue
        if state == "block":
            if ch == "*" and nxt == "/":
                out.extend("  "); i += 2; state = "code"
            else:
                out.append("\n" if ch == "\n" else " "); i += 1
            continue
        if state in ("string", "char"):
            terminator = '"' if state == "string" else "'"
            if ch == "\\\\":
                out.extend("  "); i += 2; continue
            if ch == terminator:
                state = "code"
            out.append(" "); i += 1
    if state in ("block", "string", "char"):
        raise SystemExit("FAIL: unterminated comment or literal")
    return "".join(out)

for cs_file in (root / "Assets").rglob("*.cs"):
    source = strip_comments_and_strings(cs_file.read_text(encoding="utf-8"))
    for left, right in (("{", "}"), ("(", ")"), ("[", "]")):
        depth = 0
        for char in source:
            if char == left: depth += 1
            elif char == right:
                depth -= 1
                if depth < 0:
                    raise SystemExit(f"FAIL: unmatched {right} in {cs_file}")
        if depth:
            raise SystemExit(f"FAIL: unmatched {left} in {cs_file}")

print("PASS: required sources, JSON/XML, Android configuration and C# delimiters")
