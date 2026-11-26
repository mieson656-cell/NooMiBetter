# NooMiBetter

Independent Android ragdoll-gymnastics game prototype inspired by the broad gameplay concept of bar-based ragdoll gymnastics. This is a clean-room project with its own package ID, code, assets and interface; it does not patch or bypass the original game's license checks.

## Target

- Android 10+ (API 29)
- ARM64
- Unity 2022.3 LTS
- Application ID: `com.noomibetter.game`

## Planned and prototyped systems

- Bar and tumbling gameplay modes
- Ragdoll movement, grabs, releases and tricks
- Landing grade and reward loop
- Texture Studio: generated textures and image import
- Audio Lab: custom music and catch sound import
- Cosmetic shop, wallet and inventory
- Multiplayer room relay with player state synchronization
- Map Workshop for custom obstacle layouts
- Replay recording and playback
- Performance settings and local profiles

## Repository status

The initial prototype source archive was created and locally validated. The archive contains the Unity project scripts, Android file-picker plugin, relay server, project setup scripts, documentation and branding asset.

**Important:** this repository is still being populated. The prototype is not yet a compiled or device-tested APK. Unity Editor and Android build toolchain are required to produce an installable build. Do not treat the feature list as proof that every feature is production-ready.

## Build outline

1. Open the Unity project with Unity 2022.3 LTS.
2. Install Android Build Support, Android SDK & NDK Tools, and OpenJDK through Unity Hub.
3. Switch platform to Android.
4. Set minimum API level to Android 10 / API 29 and target architecture to ARM64.
5. Build an APK and test on a physical Android device.

## Multiplayer relay

The Node.js relay server is in `Server/relay-server.js`. Run it with Node.js 18+ after installing its dependencies (if any are added in later revisions). Deploy it separately and configure the client endpoint before testing online multiplayer.

## License

Project licensing and third-party asset notices will be finalized before a public release.


## Current implementation snapshot

The repository now includes a runtime-generated prototype scene (no hand-authored scene asset required), basic bar/grab/release and flip controls, a simple landing/reward loop, workshop obstacle spawning, local profile storage, replay frame capture, texture pattern generation, texture/audio loading helpers, theme palette, and performance presets.

The Unity editor build entry point is `NooMiBetter.Editor.NooMiBuild.BuildAndroid`. The Android workflow expects a valid Unity Personal/Pro license in the GitHub Actions secret `UNITY_LICENSE`. It uploads `Build/NooMiBetter.apk` as an Actions artifact when the build succeeds.

### Known gaps before a public release

- The current gymnast is a physics-driven capsule prototype, not a full articulated ragdoll skeleton.
- The multiplayer relay is a prototype transport; client-side Unity networking integration, interpolation, reconnect handling and authoritative validation remain to be completed.
- Audio and texture byte-loading helpers exist; a polished in-game file picker and complete import UX are not yet integrated.
- The shop and inventory economy are foundational local-profile systems, not a finished content catalog.
- Replay recording exists; a complete replay browser/export UI remains outstanding.
- Unity compilation and Android device testing have not yet been performed in this environment.
