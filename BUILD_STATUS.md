# Build status

Updated: 2026-11-26

## Implemented

- Unity 2022.3 project skeleton with automatic main-scene and Android build setup.
- Articulated rigidbody ragdoll prototype: torso, head, upper/lower arms, hands, thighs and shins, connected by configurable joints.
- Bar hand constraints with grab/release, trick impulses, landing detection and reset.
- Runtime HUD, score, coin rewards, local cosmetic purchases and landing achievements.
- Procedural texture pattern generation and in-game skin application.
- Replay frame capture with a basic record/playback control.
- Performance preset (30/60 FPS).
- Multiplayer room creation/join UI, configurable relay URL, pose upload and remote player pose display.
- Node.js room relay with membership checks, rate limiting, room expiry and bounded event history.
- Docker deployment files and relay deployment notes.
- Static project validator, relay integration tests and GitHub Actions workflows for validation and Android APK builds.

## Known limitations

- The gymnast is a blockout ragdoll made from Unity primitive colliders, not a polished character model. Joint tuning and collision/animation testing are still needed.
- Multiplayer currently transmits the torso transform only. It does not synchronize all ragdoll limbs, player identity, authoritative physics, voice, chat, room leave events or reconnect recovery.
- The relay is an in-memory prototype. It needs HTTPS termination and stronger authentication/abuse controls before public use. Rooms disappear on server restart.
- The texture lab generates procedural patterns. Importing PNG/JPG through a polished Android file picker is not yet wired into the HUD.
- Audio file loading helpers exist, but user music/effect import controls are not yet integrated.
- Replay is a basic in-memory torso transform recording, not a persistent replay browser or video export.
- The workshop serializer and obstacle definitions exist, but the full map editor UI is incomplete.
- The shop currently contains a small local cosmetic palette; there is no online store or payment system.

## Build and verification status

The repository contains an Android ARM64/IL2CPP build entry point and GitHub Actions workflow. The workflow requires a valid Unity license stored as the repository secret `UNITY_LICENSE`.

No successful Unity compilation, APK build, install, or physical Android device test has been confirmed in this environment. Static validation and the Node relay tests are useful checks but do not prove that Unity compiles or that the game runs correctly on a phone.
