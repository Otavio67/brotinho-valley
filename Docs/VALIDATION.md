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
