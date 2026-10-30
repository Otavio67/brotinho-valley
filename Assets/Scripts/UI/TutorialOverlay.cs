namespace Brotinho
{
    public static class TutorialOverlay
    {
        public static void Advance(GameState state)
        {
            bool done = state.tutorial == 0 ? state.crops.Count > 0 : state.tutorial == 1 ? state.harvested > 0 : state.tutorial == 2 ? state.sold > 0 : state.tutorial == 3 ? state.buildings.Exists(b => b.definition == "chicken_coop") : state.tutorial == 4 ? state.workers.Count > 0 : state.tutorial == 5 && state.ordersCompleted > 0;
            if (done && state.tutorial < 6) state.tutorial++;
        }
    }
}
