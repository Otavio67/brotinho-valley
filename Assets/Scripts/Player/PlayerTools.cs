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
            }
        }
    }
}
