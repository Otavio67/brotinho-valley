namespace Brotinho
{
    public static class OrderManager
    {
        public static void Refresh(GameSession game)
        {
            var s = game.State; s.orders.RemoveAll(x => x.deadline <= s.elapsed);
            while (s.orders.Count < 3)
            {
                int serial = s.orderSerial++; var crop = game.Data.crops[serial % game.Data.crops.Length];
                int quantity = serial == 0 ? 2 : 2 + serial % 3;
                s.orders.Add(new OrderInstance { id = "order_" + serial, villager = game.Data.villagers[serial % game.Data.villagers.Length].id, product = crop.product, quantity = quantity, reward = game.Data.Product(crop.product).basePrice * quantity * 2 + 20, xp = 25, gems = serial % 4 == 3 ? 1 : 0, deadline = s.elapsed + 300 });
            }
        }
        public static bool Complete(GameSession game, string id)
        {
            var s = game.State; var order = s.orders.Find(x => x.id == id);
            if (order == null || order.deadline <= s.elapsed) return game.Fail("order_expired");
            if (!s.Take(order.product, order.quantity)) return game.Fail("empty_input");
            s.coins += order.reward; s.gems += order.gems; s.ordersCompleted++; game.AddXp(order.xp);
            s.orders.Remove(order); Refresh(game); game.Bus.Play(SfxEvent.Sell); game.Bus.Notify("order_complete"); game.Bus.Refresh(); return true;
        }
    }
}
