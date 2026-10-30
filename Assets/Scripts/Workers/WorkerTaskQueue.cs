namespace Brotinho
{
    public static class WorkerTaskQueue
    {
        public static bool InZone(WorkerInstance worker, int x, int y) => x >= worker.x && y >= worker.y && x < worker.x + worker.width && y < worker.y + worker.height;
        public static bool Execute(GameSession game, WorkerInstance worker)
        {
            if (worker.task == WorkerTask.Farm)
            {
                foreach (var c in game.State.crops)
                    if (InZone(worker, c.x, c.y) && c.Ready(game.Data.Crop(c.definition)) && !WeatherSystem.HarvestBlocked(game.State.elapsed) && game.State.CanAdd(game.Data.Crop(c.definition).product, game.Data.Crop(c.definition).yield)) return HarvestController.Harvest(game, c.x, c.y);
                for (int y = worker.y; y < worker.y + worker.height; y++) for (int x = worker.x; x < worker.x + worker.width; x++)
                    if (game.Grid.State(x, y, game.Data) == TileState.Empty && (game.State.Count("seed_" + worker.crop) > 0 || game.State.region == "forest" && worker.crop == "apple")) return game.Plant(worker.crop, x, y);
            }
            if (worker.task == WorkerTask.Animals)
            {
                foreach (var a in game.State.animals)
                {
                    var home = game.State.buildings.Find(b => b.id == a.building); if (home == null || !InZone(worker, home.x, home.y)) continue;
                    var data = game.Data.Animal(a.definition);
                    if (a.ready > 0 && game.State.CanAdd(data.product, a.ready)) return ProductionController.Collect(game, a);
                    if (a.feed < 2 && game.State.Count(data.feed) > 0) return ProductionController.Feed(game, a);
                }
            }
            if (worker.task == WorkerTask.Process)
            {
                foreach (var b in game.State.buildings)
                {
                    if (!InZone(worker, b.x, b.y)) continue;
                    var data = game.Data.Building(b.definition); if (data.processSeconds <= 0) continue;
                    if (b.output > 0 && game.State.CanAdd(data.output, b.output)) return ProcessingBuilding.Collect(game, b);
                    if (b.queue >= 5) continue;
                    bool hasInputs = true; foreach (var input in data.inputs) if (game.State.Count(input.id) < input.count) hasInputs = false;
                    if (hasInputs) return ProcessingBuilding.Enqueue(game, b);
                }
            }
            if (worker.task == WorkerTask.Sell)
            {
                if (!game.State.buildings.Exists(b => b.definition == "market_stall" && InZone(worker, b.x, b.y))) return false;
                foreach (var product in game.Data.products)
                    if (game.State.Count(product.id) > 0) return SellManager.Sell(game, product.id, 1);
            }
            return false;
        }
    }
}
