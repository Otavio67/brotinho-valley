namespace Brotinho
{
    public static class LevelSystem
    {
        public static int Level(int xp) => xp < 100 ? 1 : xp < 250 ? 2 : 3 + (xp - 250) / 150;
        public static int NextThreshold(int xp) { int level = Level(xp); return level == 1 ? 100 : 250 + (level - 2) * 150; }
        public static int WorkerSlots(GameState state) => 3 + state.expansions / 2 + (Level(state.xp) - 1) / 5;
    }
}
