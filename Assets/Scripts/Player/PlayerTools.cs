namespace Brotinho
{
    public enum PlayerTool { Move, Plant, Water, Harvest, Remove, Feed, Collect }
    public static class PlayerTools
    {
        public static void Apply(GameManager manager, int x, int y)
        {
            var game = manager.Game;
            switch (manager.Tool)
            {
                case PlayerTool.Plant: game.Plant(manager.SelectedCrop, x, y); break;
                case PlayerTool.Water: game.Water(x, y); break;
                case PlayerTool.Harvest: HarvestController.Harvest(game, x, y); break;
                case PlayerTool.Remove: game.Remove(x, y); break;
                case PlayerTool.Feed:
                    var house = game.Grid.Building(x, y, game.Data);
                    var animal = house == null ? null : game.State.animals.Find(a => a.building == house.id);
                    if (animal != null) ProductionController.Feed(game, animal); break;
                case PlayerTool.Collect:
                    var building = game.Grid.Building(x, y, game.Data);
                    if (building == null) break;
                    var inhabitant = game.State.animals.Find(a => a.building == building.id);
                    if (inhabitant != null) ProductionController.Collect(game, inhabitant); else ProcessingBuilding.Collect(game, building); break;
            }
        }
    }
}
