# Feature acceptance checklist

Run the automated EditMode suite first. Then enter Play from Boot. Use a fresh test save; never reset a real player's data during testing.

| Feature | Manual acceptance |
|---|---|
| Boot and scenes | Boot reaches Title without errors. Play reaches RegionSelect only for a new farm. All four region cards select a region once. Continue resumes Farm. |
| Grid | Exactly 25 tiles start unlocked. Locked land and water reject planting and construction. Expansions are adjacent 4×4 blocks. |
| Crops | Plant spends one seed; all five stages appear. Lettuce 30s/2, carrot 60s/2, wheat 90s/4, apple 180s/3 then 120s/3. No harvest before readiness. |
| Water and regions | Water speeds growth by 25% for 60s. Riverside wheat/lettuce +15%; Forest carrots −15%, free apples. Rain +20%. |
| Placement | Building preview snaps, turns 90°, changes green/red, validates full footprint. Confirm spends cost once, cancel spends nothing, undo refunds only the untouched last placement. |
| Economy | Purchases cannot overdraw. Inventory cannot exceed per-type capacity. Sell one/all uses today's deterministic price and quantity-dependent pressure. Market Stall enables worker sales. |
| Orders | Three requests, deadlines and reward previews. Completion atomically removes quantity and grants coins/XP/gems; expired orders rotate. |
| Animals | Correct housing required. One animal per house in the base version. Feeding consumes correct crop. Production pauses at zero feed. Collection respects capacity. |
| Processing | Inputs are reserved when queued. Queue caps at five including the active job. Output slot blocks further work until collected. No lost output at full inventory. |
| Workers | Hire costs 250/500/900; slot limit enforced. Assign zone and task. Workers cannot act outside their zone. Energy slows them, auto-rests at zero and recovers. |
| Offline | Close and reopen. Simulation caps at eight hours, matches fixed-step online rules, does not count time twice and reports harvest/production/sales/issues. Clock rollback grants no progress. |
| Progression | XP thresholds 100/250/+150. Claims grant once. Three daily tasks reset on the next in-game day. |
| Player | Tap walks along valid cells; obstacles cannot be crossed. Touch and mouse both work. Drag pans, long press shows info. UI touches do not affect the world. |
| UI | Open all panels from farm/menu. Lists scroll with more items than fit. Counters/progress refresh. Confirm/cancel, empty states, dialog choices and errors work. Reopen does not duplicate rows. |
| Settings | Change language; all text updates. Edit profile and avatar. Audio/music/vibration toggles persist. Notification toggle stores preference only; no permission prompts in the foundation. |
| Save | Autosave every 30s; save on pause/quit. Reopen restores all entities, queues, energy, claims, settings and fractional simulation remainder. Corrupt primary falls back to backup. Newer-version save is never overwritten. |
| Tutorial | Plant → harvest → sell → build coop → hire → complete order; objective changes only when the real action has happened. |
| Art and sound | No source image is renamed. Transparent derivatives have no green background. Crops/animals/UI atlases are present. Missing library keys warn once and use fallback. No missing-clip exceptions. |
| Mobile | 1080×1920 portrait, safe area at top/bottom, targets ≥74 reference pixels. Test notches, pause/resume, app kill/relaunch and gestures on a real Android and iOS device. |
| Performance | Profile representative dense farm on a mid-range Android; confirm 60 FPS, allocations, pool reuse, draw calls and bounded offline catch-up. Editor compilation does not establish device performance. |

Validation outcomes and commands are recorded separately in `Docs/VALIDATION.md`. Unchecked manual cases are not reported as passed.
