# Brotinho Valley

A cozy, portrait mobile farm game: plant, harvest, fulfil neighbor orders, raise animals and automate a little farm. Portuguese (Brazil) is the default language; English is included. No ads or purchases are integrated.

## Open and play

Open this repository root in **Unity 6 LTS 6000.3.27f1**. Packages are pinned in `Packages/manifest.json`. The project uses URP's 2D Renderer, uGUI/TMP and Input System. Let package import and shader compilation finish, then open `Assets/Scenes/Boot.unity` and press Play.

The Editor menu **Brotinho > Build foundation assets and scenes** regenerates default data, imported sprite rectangles, atlases, prefabs and scenes. It overwrites generated defaults; use it before customizing these assets. Gameplay state is never stored in the repository.

## Current milestone

Current implementation: v0.4.0 — workers, energy, automation and bounded offline progress. Subsequent targets: v0.5 complete UI shell; v0.6 save/recovery, localization and tutorial validation.

## Assets

All 21 supplied JPEGs live unchanged in `Assets/Art/Source`. No source filenames have been renamed. Runtime sprite names are subasset aliases recorded by `Assets/Editor/ProjectBuilder.cs`. Transparent PNG derivatives keep source basenames; only the extension and folder change. Background removal clears edge-connected chroma green, without synthesizing or redrawing the art. Sky, terrain and the UI reference sheet intentionally retain their backgrounds.

`MISSING_ASSETS.md` is generated from the SpriteLibrary requirements and lists every fallback and missing audio file. Place final art into the library using the same semantic names; gameplay code does not depend on image paths. Supplied but unused art (such as pigs) remains available for future content.

## Validation

Open Window > General > Test Runner and run EditMode and PlayMode suites. See `Docs/TEST_CHECKLIST.md` for feature checks and `Docs/SETUP.md` for scenes, data, prefabs and mobile configuration. Device performance, Android signing and iOS signing must be checked on actual target hardware before release.

## Version control

Repository: https://github.com/Otavio67/brotinho-valley (private). Default branch: `main`. Binary art/audio is tracked with Git LFS. Run `git lfs install` after cloning and `git lfs pull` before opening Unity. `Assets`, `Packages` and `ProjectSettings` are versioned; caches, local saves and credentials are excluded.
