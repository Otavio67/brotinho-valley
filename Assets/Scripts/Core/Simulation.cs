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
                bool rain = WeatherSystem.At(state.elapsed) == WeatherKind.Rain;
                foreach (var crop in state.crops) CropGrowthController.Advance(crop, game.Data.Crop(crop.definition), state.region, state.elapsed, 1, rain);
                state.elapsed++;
            }
            report.harvests = state.harvested - crops; report.production = state.produced - products;
            report.sales = state.sold - sales; report.coins = state.coins - coins;
            return report;
        }
    }
}
