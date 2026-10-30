# Foundation decisions

- Unity 6 LTS 6000.3.27f1 was installed because the existing 6000.6.6f1 Editor was not an LTS release. The official Universal 2D template supplies the render pipeline configuration.
- The original specification is implemented as a playable base project, with reusable domain services and an intentionally simple presentation. Original asset files remain untouched. The user later explicitly requested background removal; derived transparent PNGs preserve dimensions and artwork.
- Sheet filenames differ from the naming convention. Semantic SpriteLibrary aliases map import rectangles to gameplay names without renaming the originals. JPEG chroma edges and irregular frame spacing may require artist cleanup; missing assets use the neutral fallback.
- Source art includes pigs, farmhouse, well and silo, while the starting gameplay requests only chickens/cows/rabbits and ten buildings. Extra art is retained in the library, without inventing new production gameplay for it.
- A fixed 21×21 world starts with the center 5×5 unlocked. Expansion adds adjacent non-overlapping 4×4 rectangles. The finite map eventually has no valid expansion left; the UI must show this explicitly.
- A region's “growth speed” changes work performed per second, rather than subtracting a percentage from the duration. All regional, weather and watering modifiers compose multiplicatively; watering integrates only the remaining bonus window.
- One in-game day is 720 real seconds. Market prices, weather and daily tasks use this clock; no network or wall-clock daily reward dependency is introduced. Offline wall-clock time is clamped to 8 hours, with negative differences ignored.
- A worker action occurs at a bounded interval, through the same commands as manual play. Workers have no random destructive mistakes. “Careful/messy” traits affect speed and energy, preserving the cozy tone.
- A feed charge supports one full animal production cycle. New animals start with three charges. Each house initially accommodates one animal, avoiding unbounded crowding in a base project.
- Processing inputs are removed when enqueued. The active job counts toward the queue limit of five. One output batch blocks the next job until collection, so no item disappears when storage is full.
- Sell pressure applies after each unit, bottoms out at 20% of today's market price, and decays by one pressure unit per minute. Sell-all is equivalent to repeated sell-one at the same instant.
- XP: harvest 10, sale 2/item, expansion 25, orders as displayed. Level milestones granting ten gems occur every fifth level. Unlock tiers are editable on definitions.
- The Forest's otherwise unspecified wood workshop defaults to 250 coins, 1 wheat → 1 wood in 60 seconds, sell base 10, level 3 outside Forest. Fences default to 20 coins, half-price in Hillside. These values are provisional balancing choices.
- Notifications are a saved preference and an integration hook; no OS notification scheduling, ads, IAP, analytics service or online warning is needed for this offline game foundation.
- Undo is limited to the most recent untouched construction, including its refund. It cannot erase animals, queued inputs, products or storage contents. Crop removal requires confirmation in the UI.
- Saves are JSON under persistentDataPath, with a temporary write, primary/backup rotation, version guard and recovery. A future-version save or unrecoverable corruption disables automatic overwrites until the player explicitly resets.
- Gem defaults: a crop growth completion costs 1 earned gem and a new avatar costs 5. These are editable GameDatabase values; no real-money purchase or paid offline-report skip exists.
- Production timers continue while feature panels are open, as expected for a real-time farm. Leaving for Title pauses the running farm; closing/resuming the app uses the bounded offline rules.
