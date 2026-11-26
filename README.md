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
