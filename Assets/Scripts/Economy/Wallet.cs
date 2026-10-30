namespace Brotinho
{
    public static class Wallet
    {
        public static bool Spend(GameState state, long amount)
        { if (amount < 0 || state.coins < amount) return false; state.coins -= amount; return true; }
    }
}
