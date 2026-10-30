# Supplied art and next steps

The 21 originals are retained byte-for-byte in `Assets/Art/Source`, with SHA-256 hashes in `SOURCE_ASSETS.json`. There are no source renames. The user explicitly requested background removal after supplying the files; all edited outputs are derived PNGs in `Assets/Art/Transparent`.

| Supplied source | Use / library aliases |
|---|---|
| wheat.jpeg | All five wheat crop stages |
| lettuce.jpeg | All five lettuce crop stages |
| carrot.jpeg | All five carrot crop stages |
| apple_tree.jpeg | Apple tree stages and regrowth presentation |
| chicken.jpeg | Chicken idle/walk/sleep/eat/rest/funny views |
| cow.jpeg | Cow idle/walk/sleep/eat/rest/funny views |
| rabbit.jpeg | Rabbit idle/walk/sleep/eat/rest/funny views |
| my_farmer.jpeg | Farmer and temporary worker presentation |
| chicken-coop.jpeg | Coop day/night |
| barn.jpeg | Barn day/night |
| windmill.jpeg | Mill day/night |
| tileset.jpeg | Grass, soil, water and stone tiles |
| all-assets.jpeg | Logo, product/tool/currency icons, button frames, padlocks and six avatars |
| bright-sky.jpeg | Title/region-select sky background |
| afternoon-sky.jpeg | Dusk background mapped and available |
| night_sky.jpeg | Night background mapped and available |
| pig.jpeg | Imported animal poses available for future content; no pig gameplay added |
| farmhouse.jpeg | Imported day/night decoration available for future content |
| silo.jpeg | Imported day/night decoration available for future content |
| water-well.jpeg | Imported day/night decoration available for future content |
| differents-backgrounds.jpeg | Original environment reference retained and mapped |

Green backgrounds are removed by edge-connected masking, which protects enclosed green leaves and clothing. Cream backgrounds around UI cutouts are masked separately. Dimensions and source pixels inside the silhouettes remain unchanged; there is no generative redraw. Terrain and sky backgrounds remain opaque. The tiny baked button caption from the reference sheet is covered by a localized uGUI label; the original frame is retained.

## Artist handoff

- Supply the exact filenames in root `MISSING_ASSETS.md`, especially the nine missing construction types and missing audio. Missing sprites already have fallback entries and console warnings.
- Prefer native RGBA PNG exports over JPEG/chroma sheets, with consistent frame rectangles/pivots and a small transparent gutter. This improves compressed edges without any code changes.
- Keep the documented semantic sprite names. Current aliases are importer subasset names; original source files need no renaming.
- Provide distinct worker portraits/animations when ready; the existing farmer art is intentionally reused in the foundation.
- Audio hooks expect `Assets/Resources/Audio/sfx_{event}.wav`, `music_day.ogg` and `music_night.ogg`. Until supplied, all hooks safely play silence.
- Crops, Animals and UI atlases have unique names, disabled rotation and 4px padding. Preserve these settings when repacking.
