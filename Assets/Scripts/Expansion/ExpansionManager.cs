using System;
using System.Collections.Generic;
namespace Brotinho
{
    public static class ExpansionManager
    {
        public static long Cost(int owned)
        {
            long[] initial = { 300, 600, 1200, 2400, 4500, 8000 }; if (owned < initial.Length) return initial[Math.Max(0, owned)];
            double cost = 8000; for (int i = 5; i < owned; i++) cost = Math.Round(cost * 1.8, MidpointRounding.AwayFromZero);
            return (long)Math.Min(cost, long.MaxValue / 2.0);
        }
        public static List<LandBlock> Available(GameSession game)
        {
            var result = new List<LandBlock>();
            for (int y = 0; y <= GridManager.WorldSize - 4; y++) for (int x = 0; x <= GridManager.WorldSize - 4; x++)
            {
                bool occupied = false, adjacent = false;
                for (int a = x; a < x + 4; a++) for (int b = y; b < y + 4; b++) if (game.Grid.Unlocked(a, b)) occupied = true;
                if (occupied) continue;
                for (int i = 0; i < 4; i++) if (game.Grid.Unlocked(x - 1, y + i) || game.Grid.Unlocked(x + 4, y + i) || game.Grid.Unlocked(x + i, y - 1) || game.Grid.Unlocked(x + i, y + 4)) adjacent = true;
                if (adjacent) result.Add(new LandBlock(x, y));
            }
            return result;
        }
        public static bool Unlock(GameSession game, int x, int y, bool free = false)
        {
            if (!Available(game).Exists(b => b.x == x && b.y == y)) return game.Fail("invalid_placement");
            if (!free && !Wallet.Spend(game.State, Cost(game.State.expansions))) return game.Fail("not_enough_coins");
            game.State.land.Add(new LandBlock(x, y)); game.State.expansions++; game.AddXp(25); game.Bus.Play(SfxEvent.Build); game.Bus.Refresh(); return true;
        }
    }
}
