# Setup and project map

```text
Assets/
  Art/Source/          original supplied JPEG sheets
  Art/Transparent/     same-size PNG derivatives, green background removed
  Art/Atlases/         Crops, Animals, UI SpriteAtlas assets
  Editor/             reproducible data, import, prefab and scene builders
  Prefabs/            Player, Crop, Animal, Building, Worker, Panel
  Resources/          GameDatabase, SpriteLibrary, localization, default data
  Scenes/             Boot, Title, RegionSelect, Farm
  Scripts/
    Core/ Grid/ Farming/ Animals/ Buildings/ Workers/ Economy/
    Expansion/ Progression/ WeatherAndTime/ Player/ UI/ Audio/ Debug/
  Settings/           official Universal 2D template renderer/pipeline assets
  Tests/              EditMode domain tests and PlayMode smoke tests
Packages/              package manifest and dependency lock
ProjectSettings/        Unity settings, portrait, Input System, build scenes
Docs/                   setup, decisions, checklist, validation
```

## Unity

Use 6000.3.27f1 (Unity 6 LTS). Open the repository root, never the ignored reference projects under Tools. URP uses `Assets/Settings/UniversalRP.asset` and its Renderer2D asset. Keep the 2D renderer assigned in Graphics/Quality settings. Import settings and all generated assets are checked into Git with stable `.meta` files.

Input System is enabled. Runtime canvases use one EventSystem with InputSystemUIInputModule, a GraphicRaycaster, CanvasScaler 1080×1920 and a SafeArea root. Touch/pointer actions map to the same game commands. Drag threshold is 12 screen pixels; long press is 0.55s. No external account or network is required to play.

## Scenes

Build order: Boot, Title, RegionSelect, Farm. Each scene contains a camera, listener, UniversalAdditionalCameraData and Global Light 2D. GameManager creates the session and the appropriate interface on scene load, survives transitions and owns event subscriptions. Farm creates its pooled view and pointer controller. The tutorial is an overlay inside Farm.

Rendering uses the default sorting layer with explicit order: tiles −100, buildings/crops/actors sorted by world y, feedback above actors, overlay Canvas above the world. No physical collisions are needed: GridManager owns navigation and footprint blocking. Reserve additional sorting layers for final art only when necessary.

## Data assets

`Assets/Resources/Data` contains one ScriptableObject per definition, created by ProjectBuilder. Crop defaults: lettuce (30s, yield 2, seed 4), carrot (60s, 2, 8), wheat (90s, 4, 6), apple (180s first/120s regrowth, 3, 40). Product sell bases: lettuce 5, carrot 12, wheat 8, apple 25, egg 10, milk 20, wool 30, flour 15, bread 40, cheese 60, jam 55.

Animals: chicken/coop (150, 120s, egg, wheat feed), cow/barn (400, 180s, milk, wheat), rabbit/hutch (300, 240s, wool, carrot). Each feed supports one production cycle; animals start with three feed charges. Food stock and output collection are bounded.

Building costs: coop 180, barn 450, hutch 260, mill 300, bakery 500, dairy 700, kitchen 650, market stall 200, water wheel 400, storage shed 150. Mill 2 wheat→flour/20s; bakery flour+egg→bread/30s; dairy milk→cheese/45s; kitchen 3 apples→jam/40s. Queue limit five; one output batch at a time. Storage starts at 50/type; shed adds 50/type.

Worker definitions: Luna (Farming, cheerful), Bento (Animals, careful, 0.85 speed), Flora (Processing, fast/messy, 1.15 speed), Nico (Selling, grumpy). Trait differences affect speed/energy only; no random item loss. Four region definitions, five achievement definitions and three villager definitions are editable assets. GameDatabase references all of them and defines the 8h offline cap.

## Prefabs and art

Entity prefabs provide SpriteRenderers and the matching presentation behavior. Runtime views bind data instances and SpriteLibrary entries; artists can change sprites without touching simulation code. Panel prefab contains RectTransform, CanvasGroup and PanelBase. Feature panels are generated through shared UI helpers for responsive scrolling, localized text and commands.

Sprite pixels-per-unit is 150, matching the 150px tile source. Multi-frame illustrations use Bilinear filtering, no mipmaps and uncompressed alpha to preserve outlines. Fit-to-footprint view scaling accommodates differently sized source frames. Crops/animals/UI atlases disable rotation and use 4px padding. Native transparent production PNGs can replace the derived art later.

## Mobile builds

Install Android Build Support (SDK/NDK/OpenJDK) for this exact Editor, or iOS Build Support for Xcode export. Switch target in Build Profiles. Android package ID and iOS bundle ID: `com.brotinho.valley`. Portrait only. iOS final compilation/signing needs macOS and Xcode. No signing files or secrets belong in Git.
