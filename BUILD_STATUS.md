# Build status

Updated: 2026-11-26

## Implemented in the repository

- Unity 2022.3 project skeleton and Android editor setup.
- A runtime-generated playable training scene with a gymnast capsule, fixed bar anchor, floor, camera and directional light.
- On-screen controls for flip, grab/release, reset and low-power mode.
- Local score and coin persistence.
- Separate utility components for replay frame capture, local profile values, procedural texture generation, theme palette, audio loading and workshop map serialization.
- Node.js multiplayer room relay with create/join, state event relay, room membership checks, rate limiting and expiry.
- Static project validator and Node relay test suite.
- GitHub Actions workflows for project validation, relay tests and Unity Android APK build.

## Release blockers / limitations

- The playable character is currently a single rigidbody capsule. It is not yet the articulated ragdoll gymnast planned for the full game.
- The Unity client is not yet connected to the multiplayer relay. The relay is a standalone prototype service.
- Replay capture and texture/audio helper systems are not yet fully surfaced through the runtime HUD.
- A native Android file-picker bridge and complete import UX are not integrated in this repository version.
- No Unity Editor or Android SDK is installed in the current execution environment, so an actual Unity compile, APK build, install or device test has not been performed here.
- GitHub Actions Android build requires a valid Unity license configured as the repository secret `UNITY_LICENSE`.

## Verification

The project validator checks required source files, JSON/XML syntax, Android target settings and basic C# delimiter balance. The Node workflow runs syntax checks and relay integration tests. These checks are not substitutes for a Unity compile or Android device test.
