# Android build

The project targets Android 10+ (API 29), ARM64, IL2CPP and produces an APK.

## Required GitHub setup

The GitHub Actions runner must activate Unity before it can compile the project. A Unity license is not included in this repository.

1. Open the repository's GitHub Settings.
2. Open Secrets and variables → Actions.
3. Create a repository secret named `UNITY_LICENSE`.
4. Paste the license file contents exported from your own Unity account (Unity Personal or a paid license).
5. Open Actions → Android APK → Run workflow, or push a new commit to `main`.

Do not put license contents in source files, commits, issues, chat messages, or build logs. Do not use someone else's license.

## Expected output

When the license is available and the Unity build succeeds, the workflow uploads an artifact named `NooMiBetter-Android-APK` containing `NooMiBetter.apk`.

If the secret is absent, the workflow completes successfully but skips Unity compilation and artifact upload. A green workflow in that case means the missing-license condition was handled; it does not mean an APK was built.

## Local build

Open the project in Unity 2022.3.62f1, install Android Build Support including SDK/NDK and OpenJDK through Unity Hub, then use NooMiBetter → Build Android APK. The output path is `Build/NooMiBetter.apk`.
