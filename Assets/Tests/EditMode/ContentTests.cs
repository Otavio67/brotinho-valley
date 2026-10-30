using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
namespace Brotinho.Tests
{
    public class ContentTests
    {
        [Test] public void EveryLiteralUiKeyHasPortugueseAndEnglishText()
        {
            var state = GameState.New(); var locale = new Localization(state);
            foreach (string file in Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts/UI"), "*.cs"))
            foreach (Match match in Regex.Matches(File.ReadAllText(file), "L\\(\"([a-z_0-9]+)\"\\)"))
            {
                string key = match.Groups[1].Value; state.language = "pt-BR"; string pt = locale.Get(key); state.language = "en"; string en = locale.Get(key);
                Assert.That(locale.Has(key), Is.True, "Missing key: " + key); Assert.That(pt, Is.Not.Empty, "Empty Portuguese: " + key); Assert.That(en, Is.Not.Empty, "Empty English: " + key);
            }
        }
        [Test] public void EveryGameDefinitionHasLocalizedLabels()
        {
            var db = Resources.Load<GameDatabase>("GameDatabase"); var locale = new Localization(GameState.New());
            foreach (var crop in db.crops) Assert.That(locale.Has("crop_" + crop.id), Is.True, crop.id);
            foreach (var product in db.products) Assert.That(locale.Has("product_" + product.id), Is.True, product.id);
            foreach (var building in db.buildings) Assert.That(locale.Has("building_" + building.id), Is.True, building.id);
            foreach (var animal in db.animals) Assert.That(locale.Has("animal_" + animal.id), Is.True, animal.id);
            foreach (var worker in db.workers) Assert.That(locale.Has("worker_" + worker.id), Is.True, worker.id);
            foreach (var region in db.regions) { Assert.That(locale.Has("region_" + region.id), Is.True, region.id); Assert.That(locale.Has("trait_" + region.id), Is.True, region.id); }
            foreach (var a in db.achievements) Assert.That(locale.Has("achievement_" + a.id), Is.True, a.id);
            foreach (var villager in db.villagers) Assert.That(locale.Has("greeting_" + villager.id), Is.True, villager.id);
        }
        [Test] public void TransparentDerivativesKeepAlphaAndForeground()
        {
            string path = Path.Combine(Application.dataPath, "Art/Transparent/wheat.png"); var texture = new Texture2D(2, 2); texture.LoadImage(File.ReadAllBytes(path));
            Assert.That(texture.GetPixel(0, texture.height - 1).a, Is.Zero);
            Assert.That(texture.GetPixel(80, texture.height - 180).a, Is.EqualTo(1).Within(.01)); Object.DestroyImmediate(texture);
        }
        [Test] public void SpriteAliasesBindToTheIntendedSprites()
        {
            var library = Resources.Load<SpriteLibrary>("SpriteLibrary");
            Assert.That(library.Get("crop_lettuce_grown_01").name, Is.EqualTo("crop_lettuce_grown_01"));
            Assert.That(library.Get("animal_chicken_idle_down").name, Is.EqualTo("animal_chicken_idle_down"));
            Assert.That(library.Get("building_barn_day").name, Is.EqualTo("building_barn_day"));
        }
    }
}
