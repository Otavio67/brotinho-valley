# Validation evidence

Environment: Windows 11, Unity 6 LTS 6000.3.27f1. Baseline: empty repository, no existing user code or scenes. The installed Unity Personal license is active. Logs and test XML are local under ignored `Logs/` and `TestResults/`.

## v0.1.0

- `Tools/Validate.ps1 -Mode Generate`: Unity Editor compiles the runtime, editor and test assemblies and generates four scenes, definition assets, SpriteLibrary, atlases and prefabs.
- `Tools/Validate.ps1 -Mode EditMode`: 9 passed, 0 failed, 0 skipped (2026-10-30).
- Initial assembly placement and the separate URP 2D assembly reference were corrected before committing. A visual check caught JPEG import discarding alpha; output now uses a separate RGBA texture.
- Sprite-source preservation, alpha channels, serialized pipeline assignment and runtime smoke tests are verified as the foundation is completed; detailed final results appear below.
- Android/iOS device testing, signing and measured 60 FPS remain unverified until target hardware is available.
