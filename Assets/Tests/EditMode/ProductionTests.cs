using NUnit.Framework;
using UnityEngine;
namespace Brotinho.Tests
{
    public class ProductionTests
    {
        GameSession game;
        [SetUp] public void Setup() { game = new GameSession(GameState.New(), Resources.Load<GameDatabase>("GameDatabase")); game.State.region = "meadow"; game.State.coins = 10000; }
        [Test] public void HousingDiscountAndUndoRefundAreAtomic()
        {
            Assert.That(ConstructionService.Place(game, "chicken_coop", 8, 8, 0), Is.True); Assert.That(game.State.coins, Is.EqualTo(9865));
            Assert.That(ConstructionService.Undo(game), Is.True); Assert.That(game.State.coins, Is.EqualTo(10000)); Assert.That(ConstructionService.Undo(game), Is.False);
        }
        [Test] public void AnimalNeedsHousingAndOccupiedHousingCannotBeUndone()
        {
            Assert.That(ProductionController.Buy(game, "chicken"), Is.False); ConstructionService.Place(game, "chicken_coop", 8, 8, 0);
            Assert.That(ProductionController.Buy(game, "chicken"), Is.True); Assert.That(ProductionController.Buy(game, "chicken"), Is.False); Assert.That(ConstructionService.Undo(game), Is.False);
        }
        [Test] public void HungerStopsProductionUntilTheCorrectFeedIsConsumed()
        {
            var a = new AnimalInstance { definition = "chicken", feed = 1 }; game.State.animals.Add(a);
            for (int i = 0; i < 240; i++) ProductionController.Tick(game, a);
            Assert.That(a.ready, Is.EqualTo(1)); Assert.That(a.feed, Is.Zero); Assert.That(a.progress, Is.Zero);
            game.State.Add("wheat", 1); Assert.That(ProductionController.Feed(game, a), Is.True); Assert.That(game.State.Count("wheat"), Is.Zero);
        }
        [Test] public void FractionalMeadowEggBonusProducesElevenAcrossTenCycles()
        {
            var a = new AnimalInstance { definition = "chicken", feed = 10 };
            for (int i = 0; i < 1200; i++) ProductionController.Tick(game, a);
            Assert.That(a.ready, Is.EqualTo(11));
        }
        [Test] public void ProcessingReservesInputsAndStopsAtFullOutputSlot()
        {
            ConstructionService.Place(game, "mill", 8, 8, 0); var b = game.State.buildings[0]; game.State.Add("wheat", 10);
            for (int i = 0; i < 5; i++) Assert.That(ProcessingBuilding.Enqueue(game, b), Is.True);
            Assert.That(b.queue, Is.EqualTo(5)); Assert.That(ProcessingBuilding.Enqueue(game, b), Is.False); Assert.That(game.State.Count("wheat"), Is.Zero);
            for (int i = 0; i < 100; i++) ProcessingBuilding.Tick(game, b);
            Assert.That(b.output, Is.EqualTo(1)); Assert.That(b.queue, Is.EqualTo(4));
            game.State.Add("flour", 50); Assert.That(ProcessingBuilding.Collect(game, b), Is.False); Assert.That(b.output, Is.EqualTo(1));
        }
        [Test] public void WaterWheelAcceleratesProcessingOnlyInRiverside()
        {
            game.State.region = "riverside"; ConstructionService.Place(game, "water_wheel", 8, 8, 0); ConstructionService.Place(game, "mill", 10, 8, 0);
            var mill = game.State.buildings[1]; game.State.Add("wheat", 2); ProcessingBuilding.Enqueue(game, mill);
            for (int i = 0; i < 17; i++) ProcessingBuilding.Tick(game, mill);
            Assert.That(mill.output, Is.EqualTo(1));
        }
    }
}
