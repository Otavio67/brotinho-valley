using System;
namespace Brotinho
{
    // All player and worker commands go through this service. Failed commands leave state intact.
    public sealed class GameSession
    {
        public readonly GameState State;
        public readonly GameDatabase Data;
        public readonly GridManager Grid;
        public readonly EventBus Bus;
        public string LastPlacementId;
        public long LastPlacementCost;
        public GameSession(GameState state, GameDatabase data, EventBus bus = null)
        { State = state; Data = data; Grid = new GridManager(state); Bus = bus ?? new EventBus(); OrderManager.Refresh(this); AchievementManager.RefreshDaily(state); }
        public bool Plant(string cropId, int x, int y)
        {
            var crop = Data.Crop(cropId);
            if (crop == null || Grid.State(x, y, Data) != TileState.Empty) return Fail("invalid_placement");
            if (LevelSystem.Level(State.xp) < crop.unlockLevel) return Fail("locked");
            // Forest apple saplings are free and do not consume stock, as requested.
            if (!(State.region == "forest" && cropId == "apple") && !State.Take("seed_" + cropId, 1)) return Fail("no_seeds");
            State.crops.Add(new CropInstance { id = NextId("crop"), definition = cropId, x = x, y = y });
            Bus.Play(SfxEvent.Plant); Bus.Emit(x, y, SfxEvent.Plant); Bus.Refresh(); return true;
        }
        public bool Water(int x, int y)
        {
            var crop = Grid.Crop(x, y); if (crop == null) return false;
            crop.wateredUntil = State.elapsed + 60; Bus.Play(SfxEvent.Water); Bus.Emit(x, y, SfxEvent.Water); Bus.Refresh(); return true;
        }
        public bool Remove(int x, int y)
        {
            var crop = Grid.Crop(x, y); if (crop == null) return false;
            State.crops.Remove(crop); Bus.Refresh(); return true;
        }
        public bool Fail(string key) { Bus.Notify(key); Bus.Play(SfxEvent.Error); return false; }
        public string NextId(string prefix) => prefix + "_" + State.entitySequence++;
        public void AddXp(int amount)
        {
            int old = LevelSystem.Level(State.xp); State.xp += amount;
            int current = LevelSystem.Level(State.xp);
            // A level-up milestone is every fifth level; intermediate levels grant unlocks only.
            for (int level = old + 1; level <= current; level++) if (level % 5 == 0) State.gems += 10;
            if (current > old) { Bus.Play(SfxEvent.LevelUp); Bus.Notify("level_up"); }
        }
    }
}
