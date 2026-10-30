using System;
namespace Brotinho
{
    public static class AchievementManager
    {
        public static long Progress(GameState s, string metric) => metric == "harvest" ? s.harvested : metric == "sales" ? s.sold : metric == "orders" ? s.ordersCompleted : metric == "animals" ? s.animals.Count : s.expansions;
        public static bool Claim(GameSession game, AchievementData data)
        {
            if (game.State.claimedAchievements.Contains(data.id) || Progress(game.State, data.metric) < data.target) return false;
            game.State.claimedAchievements.Add(data.id); game.State.gems += data.gems; game.Bus.Play(SfxEvent.Achievement); game.Bus.Refresh(); return true;
        }
        public static void RefreshDaily(GameState s)
        {
            int day = TimeManager.Day(s.elapsed); if (s.dailyDay == day) return;
            s.dailyDay = day; s.dailyHarvestBase = s.harvested; s.dailySalesBase = s.sold; s.dailyOrdersBase = s.ordersCompleted; s.dailyClaimed.Clear();
        }
        public static long DailyProgress(GameState s, string id) => id == "harvest" ? s.harvested - s.dailyHarvestBase : id == "sales" ? s.sold - s.dailySalesBase : s.ordersCompleted - s.dailyOrdersBase;
        public static int DailyTarget(string id) => id == "orders" ? 1 : 10;
        public static bool ClaimDaily(GameSession game, string id)
        {
            RefreshDaily(game.State);
            if (id != "harvest" && id != "sales" && id != "orders") return false;
            if (game.State.dailyClaimed.Contains(id) || DailyProgress(game.State, id) < DailyTarget(id)) return false;
            game.State.dailyClaimed.Add(id); game.State.gems += 3; game.Bus.Play(SfxEvent.Achievement); game.Bus.Refresh(); return true;
        }
    }
}
