using System;
namespace Brotinho
{
    public static class ConstructionService
    {
        public static int Cost(GameSession game, BuildingBase data)
        {
            double factor = game.State.region == "meadow" && data.animalHousing ? .75 : game.State.region == "hillside" && data.id == "fence" ? .5 : 1;
            return (int)Math.Round(data.cost * factor, MidpointRounding.AwayFromZero);
        }
        public static bool Unlocked(GameSession game, BuildingBase data) => (data.region == "" || string.IsNullOrEmpty(data.region) || data.region == game.State.region) && (LevelSystem.Level(game.State.xp) >= data.unlockLevel || data.id == "wood_workshop" && game.State.region == "forest");
        public static bool Place(GameSession game, string id, int x, int y, int rotation)
        {
            var data = game.Data.Building(id); if (data == null) return false;
            if (!Unlocked(game, data)) return game.Fail("locked");
            if (!PlacementValidator.CanPlace(game.Grid, game.Data, data, x, y, rotation)) return game.Fail("invalid_placement");
            int cost = Cost(game, data); if (!Wallet.Spend(game.State, cost)) return game.Fail("not_enough_coins");
            var building = new BuildingInstance { id = game.NextId("building"), definition = id, x = x, y = y, rotation = rotation % 4 };
            game.State.buildings.Add(building); game.LastPlacementId = building.id; game.LastPlacementCost = cost;
            game.Bus.Play(SfxEvent.Build); game.Bus.Emit(x, y, SfxEvent.Build); game.Bus.Refresh(); return true;
        }
        public static bool Undo(GameSession game)
        {
            var b = game.State.buildings.Find(x => x.id == game.LastPlacementId);
            if (b == null || b.queue > 0 || b.output > 0 || game.State.animals.Exists(x => x.building == b.id)) return game.Fail("undo_unavailable");
            if (b.definition == "storage_shed" && game.State.inventory.Exists(x => x.count > game.State.Capacity - 50)) return game.Fail("storage_full");
            game.State.buildings.Remove(b); game.State.coins += game.LastPlacementCost; game.LastPlacementId = null; game.Bus.Refresh(); return true;
        }
    }
}
