using NUnit.Framework;
using UnityEngine;
namespace Brotinho.Tests
{
    public class WorkerTests
    {
        GameSession game;
        [SetUp] public void Setup() { game = new GameSession(GameState.New(), Resources.Load<GameDatabase>("GameDatabase")); game.State.region = "meadow"; game.State.coins = 10000; }
        [Test] public void HireUsesSpecifiedCostsAndRespectsSlots()
        {
            Assert.That(WorkerAI.Hire(game), Is.True); Assert.That(WorkerAI.Hire(game), Is.True); Assert.That(WorkerAI.Hire(game), Is.True); Assert.That(WorkerAI.Hire(game), Is.False);
            Assert.That(game.State.coins, Is.EqualTo(8350));
        }
        [Test] public void WorkerFarmsOnlyItsAssignedZone()
        {
            WorkerAI.Hire(game); var w = game.State.workers[0]; w.task = WorkerTask.Farm; WorkerAI.SetZone(w, 8, 8, 1, 1);
            Simulation.Advance(game, 40); Assert.That(game.State.harvested, Is.GreaterThan(0)); Assert.That(game.State.crops.TrueForAll(c => c.x == 8 && c.y == 8), Is.True);
        }
        [Test] public void ExhaustedWorkerRestsAndRecoversBeforeWorking()
        {
            WorkerAI.Hire(game); var w = game.State.workers[0]; w.task = WorkerTask.Farm; w.energy = 0;
            Simulation.Advance(game, 100); Assert.That(w.resting, Is.True); Assert.That(w.energy, Is.GreaterThan(0)); Assert.That(game.State.crops.Count, Is.Zero);
        }
        [Test] public void ChunkedOfflineAndOnlineRulesProduceIdenticalState()
        {
            WorkerAI.Hire(game); game.State.workers[0].task = WorkerTask.Farm;
            var second = new GameSession(JsonUtility.FromJson<GameState>(JsonUtility.ToJson(game.State)), game.Data);
            Simulation.Advance(game, 300); for (int i = 0; i < 300; i++) Simulation.Advance(second, 1);
            Assert.That(JsonUtility.ToJson(game.State), Is.EqualTo(JsonUtility.ToJson(second.State)));
        }
        [Test] public void SellerRequiresMarketStallInsideAssignedZone()
        {
            WorkerAI.Hire(game); var w = game.State.workers[0]; w.task = WorkerTask.Sell; game.State.Add("wheat", 4);
            Assert.That(WorkerTaskQueue.Execute(game, w), Is.False); ConstructionService.Place(game, "market_stall", 8, 8, 0);
            Assert.That(WorkerTaskQueue.Execute(game, w), Is.True); Assert.That(game.State.Count("wheat"), Is.EqualTo(3));
        }
        [Test] public void EightHourSimulationIsBoundedAndReportsHunger()
        {
            game.State.animals.Add(new AnimalInstance { definition = "chicken", feed = 0 });
            var report = Simulation.Advance(game, TimeManager.OfflineSeconds(1, 999999, game.Data.offlineCapSeconds));
            Assert.That(report.seconds, Is.EqualTo(28800)); Assert.That(game.State.elapsed, Is.EqualTo(28800)); Assert.That(report.issues, Contains.Item("hungry_animals"));
        }
    }
}
