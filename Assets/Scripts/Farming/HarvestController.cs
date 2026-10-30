namespace Brotinho
{
    public static class HarvestController
    {
        public static bool Harvest(GameSession game, int x, int y)
        {
            var state = game.State; var crop = game.Grid.Crop(x, y);
            if (crop == null) return false;
            var data = game.Data.Crop(crop.definition);
            if (!crop.Ready(data)) return game.Fail("not_ready");
            if (WeatherSystem.HarvestBlocked(state.elapsed)) return game.Fail("storm_wait");
            if (!state.Add(data.product, data.yield)) return game.Fail("storage_full");
            state.harvested += data.yield;
            if (data.regrowSeconds > 0) { crop.growth = 0; crop.regrowing = true; } else state.crops.Remove(crop);
            game.AddXp(10); game.Bus.Play(SfxEvent.Harvest); game.Bus.Emit(x, y, SfxEvent.Harvest); game.Bus.Notify("harvested"); game.Bus.Refresh(); return true;
        }
    }
}
