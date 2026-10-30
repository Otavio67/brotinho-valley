namespace Brotinho
{
    public static class ShopManager
    {
        public static bool BuySeeds(GameSession game, string id, int count = 1)
        {
            var data = game.Data.Crop(id); if (data == null || count <= 0 || count > 1000) return false;
            if (!game.State.CanAdd("seed_" + id, count)) return game.Fail("storage_full");
            if (!Wallet.Spend(game.State, (long)data.seedCost * count)) return game.Fail("not_enough_coins");
            game.State.Add("seed_" + id, count); game.Bus.Play(SfxEvent.Buy); game.Bus.Refresh(); return true;
        }
    }
}
