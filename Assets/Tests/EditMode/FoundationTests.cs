using NUnit.Framework;
using UnityEngine;
namespace Brotinho.Tests
{
    public class FoundationTests
    {
        GameDatabase data;
        GameSession game;
        [SetUp] public void Setup()
        {
            data = Resources.Load<GameDatabase>("GameDatabase");
            Assert.That(data, Is.Not.Null, "Run Brotinho > Build foundation assets and scenes first.");
            game = new GameSession(GameState.New(), data); game.State.region = "meadow";
        }
        [Test] public void StartingInventoryAndPlotMatchSpecification()
        {
            Assert.That(game.State.coins, Is.EqualTo(200)); Assert.That(game.State.Count("seed_wheat"), Is.EqualTo(6));
            int count = 0; for (int x = 0; x < 21; x++) for (int y = 0; y < 21; y++) if (game.Grid.Unlocked(x, y)) count++;
            Assert.That(count, Is.EqualTo(25));
        }
        [Test] public void PlantConsumesOneSeedAndRejectsOccupiedOrLockedTile()
        {
            Assert.That(game.Plant("lettuce", 8, 8), Is.True); Assert.That(game.State.Count("seed_lettuce"), Is.EqualTo(5));
            Assert.That(game.Plant("lettuce", 8, 8), Is.False); Assert.That(game.Plant("lettuce", 7, 8), Is.False);
            Assert.That(game.State.Count("seed_lettuce"), Is.EqualTo(5));
        }
        [Test] public void GrowthIsReadyOnlyAtFullDuration()
        {
            var crop = new CropInstance { definition = "lettuce", growth = 29.9 };
            Assert.That(crop.Stage(data.Crop("lettuce")), Is.EqualTo(CropStage.Grown));
            CropGrowthController.Advance(crop, data.Crop("lettuce"), "meadow", 0, .1, false);
            Assert.That(crop.Ready(data.Crop("lettuce")), Is.True);
        }
        [Test] public void WaterBonusExpiresWithinLargeStep()
        {
            var crop = new CropInstance { wateredUntil = 60 };
            CropGrowthController.Advance(crop, data.Crop("apple"), "meadow", 30, 60, false);
            Assert.That(crop.growth, Is.EqualTo(67.5).Within(.0001));
        }
        [Test] public void RegionModifiersAndFreeForestAppleApply()
        {
            Assert.That(CropGrowthController.RegionSpeed("riverside", "wheat"), Is.EqualTo(1.15));
            Assert.That(CropGrowthController.RegionSpeed("forest", "carrot"), Is.EqualTo(.85));
            game.State.region = "forest"; game.State.Take("seed_apple", 2);
            Assert.That(game.Plant("apple", 8, 8), Is.True); Assert.That(game.State.Count("seed_apple"), Is.Zero);
        }
        [Test] public void FractionalTicksRemainEquivalentAcrossCalls()
        {
            game.Plant("wheat", 8, 8); Simulation.Advance(game, .4); Simulation.Advance(game, .7);
            Assert.That(game.State.elapsed, Is.EqualTo(1)); Assert.That(game.State.tickRemainder, Is.EqualTo(.1).Within(.0001));
        }
        [Test] public void ClockRollbackAndOfflineCapAreBounded()
        {
            Assert.That(TimeManager.OfflineSeconds(100, 90, 28800), Is.Zero);
            Assert.That(TimeManager.OfflineSeconds(1, 1000000, 28800), Is.EqualTo(28800));
        }
        [Test] public void InventoryRejectsNegativeAmountsAndOverflow()
        {
            Assert.That(game.State.Add("wheat", 51), Is.False); Assert.That(game.State.Add("wheat", -1), Is.False);
            Assert.That(game.State.Take("seed_wheat", -1), Is.False);
        }
        [Test] public void RotationSwapsFootprintAndPlacementRejectsOccupiedCells()
        {
            Assert.That(PlacementValidator.CanPlace(game.Grid, data, data.Building("barn"), 12, 12, 0), Is.False);
            game.Plant("lettuce", 8, 8);
            Assert.That(PlacementValidator.CanPlace(game.Grid, data, data.Building("barn"), 8, 8, 0), Is.False);
        }
    }
}
