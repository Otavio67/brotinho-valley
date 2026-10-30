using NUnit.Framework;
using UnityEngine;
namespace Brotinho.Tests
{
    public class EconomyTests
    {
        GameSession game;
        [SetUp] public void Setup() { game = new GameSession(GameState.New(), Resources.Load<GameDatabase>("GameDatabase")); game.State.region = "meadow"; }
        [Test] public void PurchasesAreAtomicWhenFundsOrStorageAreMissing()
        {
            game.State.coins = 0; Assert.That(ShopManager.BuySeeds(game, "wheat"), Is.False); Assert.That(game.State.Count("seed_wheat"), Is.EqualTo(6));
            game.State.coins = 1000; Assert.That(ShopManager.BuySeeds(game, "wheat", 45), Is.False); Assert.That(game.State.coins, Is.EqualTo(1000));
        }
        [Test] public void HarvestBlocksAtCapacityAndAppleRegrowsWithoutSeed()
        {
            game.Plant("apple", 8, 8); var crop = game.State.crops[0]; crop.growth = 180;
            while (WeatherSystem.HarvestBlocked(game.State.elapsed)) game.State.elapsed++;
            game.State.Add("apple", 49); Assert.That(HarvestController.Harvest(game, 8, 8), Is.False); Assert.That(crop.growth, Is.EqualTo(180));
            game.State.Take("apple", 49); Assert.That(HarvestController.Harvest(game, 8, 8), Is.True);
            Assert.That(game.State.Count("apple"), Is.EqualTo(3)); Assert.That(crop.regrowing, Is.True); Assert.That(crop.Duration(game.Data.Crop("apple")), Is.EqualTo(120));
        }
        [Test] public void SellAllMatchesSequentialUnitSales()
        {
            var second = new GameSession(GameState.New(), game.Data); second.State.region = "meadow";
            game.State.Add("wheat", 10); second.State.Add("wheat", 10);
            SellManager.Sell(game, "wheat", 10); for (int i = 0; i < 10; i++) SellManager.Sell(second, "wheat", 1);
            Assert.That(game.State.coins, Is.EqualTo(second.State.coins)); Assert.That(game.State.Count("wheat"), Is.Zero);
        }
        [Test] public void MarketMultiplierIsBoundedAndPressureRecovers()
        {
            for (int day = 0; day < 100; day++) Assert.That(PriceTable.DailyMultiplier("apple", day), Is.InRange(.8, 1.2));
            game.State.Add("wheat", 2); SellManager.Sell(game, "wheat", 2); Assert.That(PriceTable.Pressure(game.State, "wheat"), Is.EqualTo(2));
            game.State.elapsed += 60; Assert.That(PriceTable.Pressure(game.State, "wheat"), Is.EqualTo(1));
        }
        [Test] public void OrdersGrantOnceAndRefillToThree()
        {
            var order = game.State.orders[0]; game.State.Add(order.product, order.quantity); long before = game.State.coins;
            Assert.That(OrderManager.Complete(game, order.id), Is.True); Assert.That(game.State.coins, Is.EqualTo(before + order.reward));
            Assert.That(OrderManager.Complete(game, order.id), Is.False); Assert.That(game.State.orders.Count, Is.EqualTo(3));
        }
        [Test] public void ExpansionRejectsNonAdjacentTilesAndUnlocksExactlySixteen()
        {
            game.State.coins = 10000; Assert.That(ExpansionManager.Unlock(game, 0, 0), Is.False);
            var block = ExpansionManager.Available(game)[0]; Assert.That(ExpansionManager.Unlock(game, block.x, block.y), Is.True);
            Assert.That(game.State.coins, Is.EqualTo(9700)); Assert.That(game.State.expansions, Is.EqualTo(1));
            Assert.That(ExpansionManager.Cost(6), Is.EqualTo(14400));
        }
        [Test] public void AchievementAndDailyClaimsCannotBeRepeated()
        {
            game.State.harvested = 10; var a = game.Data.achievements[0]; Assert.That(AchievementManager.Claim(game, a), Is.True); Assert.That(AchievementManager.Claim(game, a), Is.False);
            Assert.That(AchievementManager.ClaimDaily(game, "harvest"), Is.True); Assert.That(AchievementManager.ClaimDaily(game, "harvest"), Is.False); Assert.That(game.State.gems, Is.EqualTo(4));
        }
    }
}
