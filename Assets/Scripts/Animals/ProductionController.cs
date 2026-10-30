using System;
namespace Brotinho
{
    public static class ProductionController
    {
        public static bool Buy(GameSession game, string id)
        {
            var data = game.Data.Animal(id); if (data == null) return false;
            var home = game.State.buildings.Find(b => b.definition == data.housing && !game.State.animals.Exists(a => a.building == b.id));
            if (home == null) return game.Fail("needs_housing");
            if (!Wallet.Spend(game.State, data.cost)) return game.Fail("not_enough_coins");
            game.State.animals.Add(new AnimalInstance { id = game.NextId("animal"), definition = id, building = home.id }); game.Bus.Play(SfxEvent.Buy); game.Bus.Refresh(); return true;
        }
        public static void Tick(GameSession game, AnimalInstance animal)
        {
            var data = game.Data.Animal(animal.definition); if (animal.feed <= 0 || animal.ready >= 10) return;
            animal.progress++;
            if (animal.progress < data.interval) return;
            animal.progress = 0; animal.feed--; int amount = 1;
            double bonus = game.State.region == "hillside" || game.State.region == "meadow" && data.id == "chicken" ? .1 : 0;
            animal.bonusRemainder += bonus;
            if (animal.bonusRemainder >= .999999) { amount++; animal.bonusRemainder -= 1; }
            animal.ready += amount; game.State.produced += amount;
        }
        public static bool Feed(GameSession game, AnimalInstance animal)
        {
            if (animal.feed >= 10) return game.Fail("queue_full");
            if (!game.State.Take(game.Data.Animal(animal.definition).feed, 1)) return game.Fail("empty_input");
            animal.feed++; game.Bus.Play(SfxEvent.Notification); game.Bus.Refresh(); return true;
        }
        public static bool Collect(GameSession game, AnimalInstance animal)
        {
            if (animal.ready <= 0) return false;
            if (!game.State.Add(game.Data.Animal(animal.definition).product, animal.ready)) return game.Fail("storage_full");
            animal.ready = 0; game.Bus.Play(SfxEvent.Notification); game.Bus.Refresh(); return true;
        }
    }
}
