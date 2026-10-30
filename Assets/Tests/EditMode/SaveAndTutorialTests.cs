using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
namespace Brotinho.Tests
{
    public class SaveAndTutorialTests
    {
        string folder;
        SaveSystem saves;
        [SetUp] public void Setup() { folder = Path.Combine(Path.GetTempPath(), "BrotinhoTests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder); saves = new SaveSystem(folder); }
        [TearDown] public void Cleanup()
        {
            if (!Path.GetFullPath(folder).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Test cleanup outside temp directory.");
            Directory.Delete(folder, true);
        }
        [Test] public void JsonRoundTripPreservesQueuesEnergySettingsAndFractions()
        {
            var state = GameState.New(); state.region = "forest"; state.language = "en"; state.tickRemainder = .75;
            state.buildings.Add(new BuildingInstance { id = "b", definition = "mill", queue = 3, progress = 7 });
            state.workers.Add(new WorkerInstance { definition = "luna", task = WorkerTask.Farm, energy = 32.5 });
            state.claimedAchievements.Add("first_harvest"); Assert.That(saves.Save(state, 123456), Is.True);
            Assert.That(JsonUtility.ToJson(saves.Load()), Is.EqualTo(JsonUtility.ToJson(state)));
        }
        [Test] public void CorruptPrimaryRecoversAndPreservesGoodBackupDuringRepair()
        {
            var s = GameState.New(); s.coins = 400; saves.Save(s, 1); s.coins = 500; saves.Save(s, 2);
            File.WriteAllText(Path.Combine(folder, "brotinho-save.json"), "broken"); var recovered = saves.Load();
            Assert.That(recovered.coins, Is.EqualTo(400)); Assert.That(saves.LastError, Is.EqualTo("save_recovered"));
            recovered.coins = 600; Assert.That(saves.Save(recovered, 3), Is.True);
            var backup = JsonUtility.FromJson<GameState>(File.ReadAllText(Path.Combine(folder, "brotinho-save.json.bak")));
            Assert.That(backup.coins, Is.EqualTo(400));
        }
        [Test] public void FutureVersionNeverOverwritesUserFiles()
        {
            var s = GameState.New(); s.version = 999; string json = JsonUtility.ToJson(s); string path = Path.Combine(folder, "brotinho-save.json"); File.WriteAllText(path, json);
            saves.Load(); Assert.That(saves.WriteBlocked, Is.True); Assert.That(saves.Save(GameState.New(), 1), Is.False); Assert.That(File.ReadAllText(path), Is.EqualTo(json));
        }
        [Test] public void UnrecoverableCorruptionBlocksWritesUntilExplicitReset()
        {
            string path = Path.Combine(folder, "brotinho-save.json"); File.WriteAllText(path, "{}");
            saves.Load(); Assert.That(saves.WriteBlocked, Is.True); Assert.That(saves.Save(GameState.New(), 1), Is.False);
            saves.Reset(); Assert.That(saves.Save(GameState.New(), 1), Is.True);
        }
        [Test] public void VersionZeroMigratesWithoutDiscardingProgress()
        {
            var s = GameState.New(); s.version = 0; s.coins = 1234; File.WriteAllText(Path.Combine(folder, "brotinho-save.json"), JsonUtility.ToJson(s));
            var loaded = saves.Load(); Assert.That(loaded.version, Is.EqualTo(1)); Assert.That(loaded.coins, Is.EqualTo(1234));
        }
        [Test] public void MalformedNegativeInventoryCannotBeLoaded()
        {
            var s = GameState.New(); s.inventory[0].count = -1; File.WriteAllText(Path.Combine(folder, "brotinho-save.json"), JsonUtility.ToJson(s));
            saves.Load(); Assert.That(saves.WriteBlocked, Is.True);
        }
        [Test] public void LanguageSwitchChangesKeysWithoutChangingGameData()
        {
            var s = GameState.New(); var text = new Localization(s); Assert.That(text.Get("harvest"), Is.EqualTo("Colher"));
            s.language = "en"; Assert.That(text.Get("harvest"), Is.EqualTo("Harvest")); Assert.That(s.coins, Is.EqualTo(200));
        }
        [Test] public void TutorialRequiresActionsInTheSpecifiedSequence()
        {
            var game = new GameSession(GameState.New(), Resources.Load<GameDatabase>("GameDatabase")); game.State.region = "meadow"; game.State.coins = 10000;
            TutorialOverlay.Advance(game.State); Assert.That(game.State.tutorial, Is.Zero);
            game.Plant("lettuce", 8, 11); TutorialOverlay.Advance(game.State); Assert.That(game.State.tutorial, Is.EqualTo(1));
            game.State.crops[0].growth = 30; HarvestController.Harvest(game, 8, 11); TutorialOverlay.Advance(game.State); Assert.That(game.State.tutorial, Is.EqualTo(2));
            SellManager.Sell(game, "lettuce", 2); TutorialOverlay.Advance(game.State); Assert.That(game.State.tutorial, Is.EqualTo(3));
            ConstructionService.Place(game, "chicken_coop", 8, 8, 0); TutorialOverlay.Advance(game.State); Assert.That(game.State.tutorial, Is.EqualTo(4));
            WorkerAI.Hire(game); TutorialOverlay.Advance(game.State); Assert.That(game.State.tutorial, Is.EqualTo(5));
            var order = game.State.orders[0]; game.State.Add(order.product, order.quantity); OrderManager.Complete(game, order.id); TutorialOverlay.Advance(game.State); Assert.That(game.State.tutorial, Is.EqualTo(6));
        }
    }
}
