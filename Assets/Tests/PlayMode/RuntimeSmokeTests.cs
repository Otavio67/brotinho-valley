using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Brotinho.Tests
{
    public class RuntimeSmokeTests
    {
        [UnitySetUp] public IEnumerator Setup()
        {
            if (GameManager.Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
            GameManager.Instance.ResetGame(); yield return null;
            GameManager.Instance.ChooseRegion("meadow"); yield return null; yield return null;
            Assert.That(Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF"), Is.Not.Null, "TMP resources must be imported; invisible text is not a passing UI.");
        }
        [UnityTest] public IEnumerator BootAndRegionSelectionUseTheRealMenuButtons()
        {
            var manager = GameManager.Instance; manager.ResetGame(); yield return null;
            SceneLoader.Load("Boot"); yield return null;
            Assert.That(Object.FindAnyObjectByType<GameUI>().GetComponentsInChildren<TextMeshProUGUI>().Any(t => t.text == manager.Text.Get("loading")), Is.True);
            yield return new WaitForSecondsRealtime(.35f); Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Title"));
            var play = Object.FindAnyObjectByType<GameUI>().GetComponentsInChildren<UnityEngine.UI.Button>().First(b => b.GetComponentInChildren<TextMeshProUGUI>().text == manager.Text.Get("play")); play.onClick.Invoke(); yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RegionSelect"));
            var region = Object.FindAnyObjectByType<GameUI>().GetComponentsInChildren<UnityEngine.UI.Button>().First(b => b.GetComponentInChildren<TextMeshProUGUI>().text == manager.Text.Get("region_riverside")); region.onClick.Invoke(); yield return null;
            Assert.That(manager.Game.State.region, Is.EqualTo("riverside")); Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Farm"));
        }
        [UnityTest] public IEnumerator WorkerTaskDropdownCreatesSelectableOptions()
        {
            var manager = GameManager.Instance; manager.Game.State.coins = 10000; WorkerAI.Hire(manager.Game);
            var ui = Object.FindAnyObjectByType<GameUI>(); ui.Open("workers"); yield return null;
            ui.GetComponentsInChildren<UnityEngine.UI.Button>().Last(b => b.GetComponentInChildren<TextMeshProUGUI>().text == manager.Text.Get("details")).onClick.Invoke(); yield return new WaitForSecondsRealtime(.25f);
            var dropdown = ui.GetComponentInChildren<TMP_Dropdown>(); Assert.That(dropdown, Is.Not.Null); dropdown.Show(); yield return null;
            var option = Object.FindObjectsByType<UnityEngine.UI.Toggle>(FindObjectsSortMode.None).First(t => t.GetComponentInChildren<TextMeshProUGUI>() != null && t.GetComponentInChildren<TextMeshProUGUI>().text == manager.Text.Get("task_farm"));
            option.isOn = true; Assert.That(manager.Game.State.workers[0].task, Is.EqualTo(WorkerTask.Farm)); dropdown.Hide();
        }
        [UnityTest] public IEnumerator AllFeaturePanelsOpenWithoutRuntimeErrors()
        {
            var ui = Object.FindAnyObjectByType<GameUI>(); Assert.That(ui, Is.Not.Null);
            foreach (string panel in new[] { "build", "shop", "storage", "inventory", "sell", "workers", "worker_detail", "animals", "processing", "processor", "orders", "expansion", "achievements", "daily", "profile", "settings", "pause", "afk", "dialog", "credits", "error", "debug" })
            {
                ui.Open(panel); yield return null;
                Assert.That(GameUI.ModalOpen, Is.True, panel); Assert.That(GameObject.Find("Panel_" + panel), Is.Not.Null, panel);
            }
        }
        [UnityTest] public IEnumerator PlantHarvestSellAndRemoveConfirmationAreFunctional()
        {
            var manager = GameManager.Instance; var game = manager.Game; var ui = Object.FindAnyObjectByType<GameUI>();
            Assert.That(game.Plant("lettuce", 8, 11), Is.True); Simulation.Advance(game, 35);
            while (WeatherSystem.HarvestBlocked(game.State.elapsed)) Simulation.Advance(game, 1);
            Assert.That(HarvestController.Harvest(game, 8, 11), Is.True); long before = game.State.coins;
            ui.Open("inventory"); yield return null;
            var sell = ui.GetComponentsInChildren<UnityEngine.UI.Button>().First(b => b.GetComponentInChildren<TextMeshProUGUI>().text == manager.Text.Get("sell_all"));
            sell.onClick.Invoke(); Assert.That(game.State.Count("lettuce"), Is.Zero); Assert.That(game.State.coins, Is.GreaterThan(before));
            game.Plant("carrot", 8, 11); GameUI.RequestRemove(manager, 8, 11); yield return null;
            var cancel = ui.GetComponentsInChildren<UnityEngine.UI.Button>().Last(b => b.GetComponentInChildren<TextMeshProUGUI>().text == manager.Text.Get("cancel")); cancel.onClick.Invoke(); Assert.That(game.Grid.Crop(8, 11), Is.Not.Null);
            yield return new WaitForSecondsRealtime(.25f); GameUI.RequestRemove(manager, 8, 11); yield return null;
            var confirm = ui.GetComponentsInChildren<UnityEngine.UI.Button>().Last(b => b.GetComponentInChildren<TextMeshProUGUI>().text == manager.Text.Get("confirm")); confirm.onClick.Invoke(); Assert.That(game.Grid.Crop(8, 11), Is.Null);
        }
        [UnityTest] public IEnumerator CapturePortraitFarmAndTitle()
        {
            var manager = GameManager.Instance; var game = manager.Game; game.State.coins = 10000;
            ConstructionService.Place(game, "chicken_coop", 8, 8, 0); ProductionController.Buy(game, "chicken");
            game.Plant("lettuce", 8, 11); game.Plant("wheat", 9, 11); game.Plant("carrot", 10, 11); game.Plant("apple", 11, 11);
            Simulation.Advance(game, 60); game.Bus.Refresh(); yield return null; yield return null;
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", "TestResults"));
            File.WriteAllText(Path.Combine(Application.dataPath, "..", "TestResults", "RuntimeSprites.txt"), string.Join("\n", Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Where(x => x.sprite != null && x.gameObject.activeSelf).Select(x => x.name + " | " + x.sprite.name + " | tex=" + x.sprite.texture.name + " rect=" + x.sprite.rect + " uv=" + x.sprite.textureRect + " mat=" + x.sharedMaterial.name)));
            Capture("Farm"); SceneLoader.Load("Title"); yield return null; yield return null; Capture("Title");
        }
        static void Capture(string name)
        {
            var camera = Camera.main; var ui = Object.FindAnyObjectByType<GameUI>(); var canvas = ui.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 5;
            var target = new RenderTexture(1080, 1920, 24); camera.targetTexture = target; Canvas.ForceUpdateCanvases();
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = target });
            var old = RenderTexture.active; RenderTexture.active = target; var texture = new Texture2D(1080, 1920, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0); texture.Apply();
            string folder = Path.Combine(Application.dataPath, "..", "TestResults"); Directory.CreateDirectory(folder); File.WriteAllBytes(Path.Combine(folder, "Preview-" + name + ".png"), texture.EncodeToPNG());
            camera.targetTexture = null; RenderTexture.active = old; canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Object.Destroy(texture); Object.Destroy(target);
        }
    }
}
