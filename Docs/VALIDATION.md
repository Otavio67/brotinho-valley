# Validation evidence

Environment: Windows 11, Unity 6 LTS 6000.3.27f1. Baseline: empty repository, no existing user code or scenes. The installed Unity Personal license is active. Logs and test XML are local under ignored `Logs/` and `TestResults/`.

## v0.1.0

- `Tools/Validate.ps1 -Mode Generate`: Unity Editor compiles the runtime, editor and test assemblies and generates four scenes, definition assets, SpriteLibrary, atlases and prefabs.
- `Tools/Validate.ps1 -Mode EditMode`: 9 passed, 0 failed, 0 skipped (2026-10-30).
- Initial assembly placement and the separate URP 2D assembly reference were corrected before committing. A visual check caught JPEG import discarding alpha; output now uses a separate RGBA texture.
- Sprite-source preservation, alpha channels, serialized pipeline assignment and runtime smoke tests are verified as the foundation is completed; detailed final results appear below.
- Android/iOS device testing, signing and measured 60 FPS remain unverified until target hardware is available.

## v0.2.0

- `Tools/Validate.ps1 -Mode EditMode`: Unity compilation succeeded; 16 passed, 0 failed, 0 skipped.
- New cases cover atomic purchasing/harvest capacity, apple regrowth, sell-all equivalence, bounded market multipliers, pressure recovery, one-time order rewards, expansion adjacency/cost and repeat-claim rejection.

## v0.3.0

- `Tools/Validate.ps1 -Mode EditMode`: Unity compilation succeeded; 22 passed, 0 failed, 0 skipped.
- New cases prove housing restrictions and discounts, exact undo refunds, hunger pause/feed consumption, fractional regional yields, reserved processing inputs, full output blocking and water-wheel acceleration.

## v0.4.0

- `Tools/Validate.ps1 -Mode EditMode`: Unity compilation succeeded; 28 passed, 0 failed, 0 skipped.
- New cases prove hire costs/slot limits, zone-restricted harvesting/planting, automatic energy recovery, deterministic 300×1s versus 1×300s simulation, required market access and bounded 8-hour catch-up with hunger reporting.

## v0.5.0

- EditMode: 28 passed. PlayMode: 5 passed, 0 failed, 0 skipped. The PlayMode run recompiles all affected assemblies before execution.
- Runtime checks open the full feature panel set, exercise Boot → Title → RegionSelect → Farm through actual menu buttons, use the worker dropdown, and verify planting/harvesting/selling plus remove confirmation/cancel.
- Actual 1080×1920 URP captures were visually inspected: `TestResults/Preview-Farm.png` and `Preview-Title.png`.
- Visual QA caught missing TMP resources and atlas-name collisions that compilation alone did not catch. Essential resources now finish importing through the supported callback before batch exit; all atlases have unique names. Screens render legible text and the intended sprites.
- All 21 original files match the user-provided source SHA-256 byte-for-byte; hashes are in `Docs/SOURCE_ASSETS.json`.

## v0.6.0 — final result

**Unity foundation ready with documented platform limitations.**

- Unity 6000.3.27f1 runtime/editor/test assemblies compile with no C# errors.
- `Tools/Validate.ps1 -Mode EditMode`: **42 passed, 0 failed, 0 skipped**.
- `Tools/Validate.ps1 -Mode PlayMode`: **5 passed, 0 failed, 0 skipped**.
- New coverage includes JSON round trips with queues/energy/settings/fractions, corrupt-primary recovery while retaining the good backup, future-version write protection, explicit reset, v0 migration, invalid inventory rejection, both languages, ordered tutorial advancement, one-time avatar purchases, crop boosts, literal/definition localization keys, PNG alpha and sprite alias identity.
- PlayMode tests are isolated through the test-runner setup/cleanup environment marker and a batch-run guard. They assert isolation before reset and never write a real player save. Save tests create and remove their own unique OS-temp directory.
- The final portrait farm/title captures were rendered through the URP render-request API. Transparent crops, farmer, animal and building art render correctly. Missing construction/icon assets emit expected one-time fallback warnings.
- The source archive contains 21 untouched originals and 57 transparent derivative PNGs. `MISSING_ASSETS.md` lists 23 sprite filenames (18 building state files and 5 icons), plus 14 SFX and 2 music tracks.

### Platform evidence and limits

Android Build Support installation was attempted for this exact LTS Editor. The first attempt returned `ELEVATION_CANCELLED` (the Windows elevation prompt was cancelled or timed out). The documented `--no-elevate` retry returned `ELEVATION_REQUIRED`: installation under `C:\Program Files\Unity\Hub\Editor\6000.3.27f1` needs administrator permission. No APK was generated. These are Windows installer permission errors, not C# build failures.

The reproducible build entry point is `Brotinho.Editor.MobileBuild.Android`: four scenes, portrait, ARM64, IL2CPP, minimum Android API 26, development APK at `Builds/Android/BrotinhoValley-debug.apk`. Run it after installing Android Build Support and its SDK/NDK/OpenJDK for 6000.3.27f1.

iOS compilation/signing requires the corresponding module and a macOS/Xcode environment; it was not run. No Android/iOS device was connected, so touch gestures, safe-area behavior on physical notches, pause/resume under the OS, signing, battery usage and measured 60 FPS are **not verified**. The project is a base for device validation, not a store-release certification.
