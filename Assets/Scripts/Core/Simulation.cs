using System;
namespace Brotinho
{
    public static class Simulation
    {
        // Fixed one-second steps and persisted fractional remainder make chunked/offline simulation equivalent.
        // Maximum offline work is bounded to 28,800 steps, independent of system clock jumps.
        public static OfflineReport Advance(GameSession game, double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            var state = game.State;
            var report = new OfflineReport { seconds = seconds };
            long crops = state.harvested, products = state.produced, sales = state.sold, coins = state.coins;
            double accumulated = state.tickRemainder + seconds;
            int ticks = (int)Math.Min(int.MaxValue, Math.Floor(accumulated));
            state.tickRemainder = accumulated - ticks;
            for (int i = 0; i < ticks; i++)
            {
                AchievementManager.RefreshDaily(state);
                bool rain = WeatherSystem.At(state.elapsed) == WeatherKind.Rain;
                foreach (var crop in state.crops) CropGrowthController.Advance(crop, game.Data.Crop(crop.definition), state.region, state.elapsed, 1, rain);
                foreach (var animal in state.animals) ProductionController.Tick(game, animal);
                foreach (var building in state.buildings) ProcessingBuilding.Tick(game, building);
                foreach (var worker in state.workers) WorkerAI.Tick(game, worker);
                state.elapsed++;
                OrderManager.Refresh(game);
            }
            report.harvests = state.harvested - crops; report.production = state.produced - products;
            report.sales = state.sold - sales; report.coins = state.coins - coins;
            foreach (var animal in state.animals) if (animal.feed == 0) report.Issue("hungry_animals");
            foreach (var item in state.inventory) if (item.count >= state.Capacity) report.Issue("storage_full");
            foreach (var building in state.buildings)
            {
                var data = game.Data.Building(building.definition); if (data.processSeconds <= 0) continue;
                if (building.output > 0) report.Issue("output_waiting"); else if (building.queue == 0) report.Issue("empty_input");
            }
            foreach (var worker in state.workers) if (worker.task == WorkerTask.Farm && state.Count("seed_" + worker.crop) == 0 && !(state.region == "forest" && worker.crop == "apple")) report.Issue("no_seeds");
            return report;
        }
    }
}
