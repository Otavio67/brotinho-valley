namespace Brotinho
{
    public static class SellManager
    {
        public static bool Sell(GameSession game, string product, int amount)
        {
            if (amount <= 0 || game.Data.Product(product) == null || game.State.Count(product) < amount) return false;
            long value = PriceTable.Quote(game, product, amount); double pressure = PriceTable.Pressure(game.State, product);
            game.State.Take(product, amount); game.State.coins += value; game.State.sold += amount;
            var sale = game.State.market.Find(x => x.product == product);
            if (sale == null) { sale = new MarketSale { product = product }; game.State.market.Add(sale); }
            sale.pressure = pressure + amount; sale.lastTime = game.State.elapsed;
            game.AddXp(amount * 2); game.Bus.Play(SfxEvent.Sell); game.Bus.Refresh(); return true;
        }
    }
}
