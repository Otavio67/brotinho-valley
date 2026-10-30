# Setup and project map

```text
Assets/
  Art/Source/          original supplied JPEG sheets
  Art/Transparent/     same-size PNG derivatives, green background removed
  Art/Atlases/         Crops, Animals, UI SpriteAtlas assets
  Editor/             reproducible data, import, prefab and scene builders
  Resources/          GameDatabase, SpriteLibrary, localization, default data
    Prefabs/          Player, Crop, Animal, Building, Worker, Panel
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

GameDatabase also defines earned-gem prices: 1 gem to complete one crop's growth and 5 gems for an additional avatar. The first of six supplied avatars is free. Purchased avatars persist; crop boosts cannot charge a crop already ready. There are no real-money purchases.

Starting state: 200 coins, 0 gems, 6 wheat seeds, 6 lettuce seeds, 4 carrot seeds and 2 apple saplings. First three hires cost 250/500/900 coins; later costs grow ×1.8. Initial worker slots: 3, plus one per two expansions and one per five earned levels. Expansion prices: 300/600/1200/2400/4500/8000, then rounded previous ×1.8. Level 2 requires 100 XP, level 3 requires 250, then +150 per level. A level milestone is every fifth level and grants 10 gems.

| Achievement asset ID | Metric / target | Reward |
|---|---|---|
| first_harvest | Harvest 1 item | 1 gem |
| merchant | Sell 20 items | 1 gem |
| neighbor | Complete 1 order | 1 gem |
| rancher | Own 3 animals | 1 gem |
| explorer | Unlock 1 expansion | 1 gem |

Daily tasks reset every 720 simulation seconds: harvest 10, sell 10, complete 1 order; each grants 3 gems once. Villagers Rosa, Tomás and Íris rotate through three crop orders, with a 300-second deadline and displayed coins/XP/gem rewards.

| Region asset ID | Gameplay trait |
|---|---|
| riverside | Wheat/lettuce growth ×1.15, additional water, water wheel for processing ×1.2 |
| hillside | Apple sell value ×1.2, animal yield +10%, fences half price |
| forest | Free apple planting without sapling consumption, carrot growth ×0.85, early wood workshop |
| meadow | Animal housing ×0.75 cost, egg yield +10% |

## Prefabs and art

Entity prefabs provide SpriteRenderers and the matching presentation behavior. Runtime views bind data instances and SpriteLibrary entries; artists can change sprites without touching simulation code. Panel prefab contains RectTransform, CanvasGroup and PanelBase. Feature panels are generated through shared UI helpers for responsive scrolling, localized text and commands.

| Prefab under Assets/Resources/Prefabs | Components / binding |
|---|---|
| Player | SpriteRenderer + PlayerMovement; bound to the session's farmer coordinates and grid route |
| Crop | SpriteRenderer; FarmView binds CropInstance, stage sprite, sway, glow and bounce |
| Animal | SpriteRenderer + AnimalBehavior; bound to AnimalInstance and its housing zone |
| Building | SpriteRenderer; FarmView binds footprint, rotation and day/night sprite; production lives in the session |
| Worker | SpriteRenderer; FarmView binds WorkerInstance while WorkerAI/WorkerTaskQueue advance deterministic tasks |
| Panel | RectTransform + CanvasGroup + PanelBase; common animation and interaction root |

All UI strings come from the tab-separated `Assets/Resources/Localization.txt`: key, pt-BR, en. User profile names are user data and remain unchanged when switching language. New content definitions use stable IDs and corresponding localization/sprite keys.

Sprite pixels-per-unit is 150, matching the 150px tile source. Multi-frame illustrations use Bilinear filtering, no mipmaps and uncompressed alpha to preserve outlines. Fit-to-footprint view scaling accommodates differently sized source frames. Crops/animals/UI atlases disable rotation and use 4px padding. Native transparent production PNGs can replace the derived art later.

## Mobile builds

Install Android Build Support (SDK/NDK/OpenJDK) for this exact Editor, or iOS Build Support for Xcode export. Switch target in Build Profiles. Android package ID and iOS bundle ID: `com.brotinho.valley`. Portrait only. iOS final compilation/signing needs macOS and Xcode. No signing files or secrets belong in Git.

After installing the Android module, run Unity in batch mode with `-executeMethod Brotinho.Editor.MobileBuild.Android -quit` and the repository as `-projectPath`, or invoke that method through an Editor tool. It selects ARM64/IL2CPP and exports the development APK under ignored `Builds/Android`. The source project has been compiled and tested; the current machine's administrator restriction prevented APK export during this task.
