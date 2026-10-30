using System;
namespace Brotinho
{
    public static class PriceTable
    {
        public static double DailyMultiplier(string product, int day)
        {
            uint hash = 2166136261; foreach (char c in product) hash = (hash ^ c) * 16777619;
            hash = (hash ^ (uint)day) * 16777619; return (800 + hash % 401) / 1000.0;
        }
        public static double Pressure(GameState state, string product)
        {
            var sale = state.market.Find(x => x.product == product);
            if (sale == null || TimeManager.Day(state.elapsed) != TimeManager.Day(sale.lastTime)) return 0;
            return Math.Max(0, sale.pressure - (state.elapsed - sale.lastTime) / 60);
        }
        public static int Unit(GameSession game, string product, int offset = 0)
        {
            var data = game.Data.Product(product); if (data == null) return 0;
            double regional = game.State.region == "hillside" && product == "apple" ? 1.2 : 1;
            return Math.Max(1, (int)Math.Round(data.basePrice * DailyMultiplier(product, TimeManager.Day(game.State.elapsed)) * regional * Math.Max(.2, 1 - .05 * (Pressure(game.State, product) + offset)), MidpointRounding.AwayFromZero));
        }
        public static long Quote(GameSession game, string product, int amount)
        { long total = 0; for (int i = 0; i < amount; i++) total += Unit(game, product, i); return total; }
    }
}
