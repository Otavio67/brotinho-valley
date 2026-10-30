using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.U2D;
namespace Brotinho.Editor
{
    public static class ProjectBuilder
    {
        const string Root = "Assets/Resources/";
        static readonly List<SpriteLibrary.Entry> art = new List<SpriteLibrary.Entry>();
        static readonly List<string> missing = new List<string>();
        [MenuItem("Brotinho/Build foundation assets and scenes")]
        public static void Build()
        {
            Directory.CreateDirectory(Root + "Data"); Directory.CreateDirectory("Assets/Scenes"); Directory.CreateDirectory("Assets/Prefabs"); Directory.CreateDirectory("Assets/Art/Atlases");
            BackgroundRemoval.Run(); BuildArt(); BuildData(); BuildScenes(); BuildPrefabs();
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP.asset");
            GraphicsSettings.defaultRenderPipeline = pipeline; QualitySettings.renderPipeline = pipeline;
            PlayerSettings.companyName = "Brotinho"; PlayerSettings.productName = "Brotinho Valley";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth = 1080; PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false; PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.brotinho.valley");
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS, "com.brotinho.valley");
            // TMP resource import without blocking the Editor with a modal package dialog.
            if (Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF") == null) TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            UnityEngine.Debug.Log("BROTINHO_BUILD_OK");
        }
        static T Save<T>(T asset, string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null) { AssetDatabase.CreateAsset(asset, path); return asset; }
            EditorUtility.CopySerialized(asset, existing); UnityEngine.Object.DestroyImmediate(asset); EditorUtility.SetDirty(existing); return existing;
        }
        static T Definition<T>(string id, Action<T> configure) where T : ScriptableObject
        {
            var item = ScriptableObject.CreateInstance<T>(); item.name = id; configure(item); return Save(item, Root + "Data/" + typeof(T).Name + "_" + id + ".asset");
        }
        static void BuildData()
        {
            var db = ScriptableObject.CreateInstance<GameDatabase>();
            string[] ids = { "lettuce", "carrot", "wheat", "apple" }; int[] costs = { 4, 8, 6, 40 }, yields = { 2, 2, 4, 3 }, times = { 30, 60, 90, 180 };
            db.crops = ids.Select((id, i) => Definition<CropBase>(id, d => { d.id = id; d.product = id; d.seedCost = costs[i]; d.yield = yields[i]; d.growthSeconds = times[i]; d.regrowSeconds = id == "apple" ? 120 : 0; })).ToArray();
            string[] products = { "lettuce", "carrot", "wheat", "apple", "egg", "milk", "wool", "flour", "bread", "cheese", "jam", "wood" };
            int[] prices = { 5, 12, 8, 25, 10, 20, 30, 15, 40, 60, 55, 10 };
            db.products = products.Select((id, i) => Definition<ProductData>(id, d => { d.id = id; d.basePrice = prices[i]; })).ToArray();
            string[] buildings = { "chicken_coop", "barn", "hutch", "mill", "bakery", "dairy", "kitchen", "market_stall", "water_wheel", "storage_shed", "wood_workshop", "fence" };
            int[] buildingCosts = { 180, 450, 260, 300, 500, 700, 650, 200, 400, 150, 250, 20 };
            db.buildings = buildings.Select((id, i) => Definition<BuildingBase>(id, d =>
            {
                d.id = id; d.cost = buildingCosts[i]; d.animalHousing = i < 3; d.size = id == "fence" ? Vector2Int.one : new Vector2Int(2, 2); d.decoration = id == "fence";
                d.unlockLevel = id == "dairy" || id == "kitchen" ? 3 : id == "bakery" ? 2 : 1;
                if (id == "water_wheel") d.region = "riverside";
                if (id == "mill") { d.inputs = new[] { new ItemStack("wheat", 2) }; d.output = "flour"; d.processSeconds = 20; }
                if (id == "bakery") { d.inputs = new[] { new ItemStack("flour", 1), new ItemStack("egg", 1) }; d.output = "bread"; d.processSeconds = 30; }
                if (id == "dairy") { d.inputs = new[] { new ItemStack("milk", 1) }; d.output = "cheese"; d.processSeconds = 45; }
                if (id == "kitchen") { d.inputs = new[] { new ItemStack("apple", 3) }; d.output = "jam"; d.processSeconds = 40; }
                // Cozy default for the unspecified Forest wood producer: 1 wheat -> 1 wood in 60 seconds.
                if (id == "wood_workshop") { d.inputs = new[] { new ItemStack("wheat", 1) }; d.output = "wood"; d.processSeconds = 60; d.unlockLevel = 3; }
            })).ToArray();
            string[] animals = { "chicken", "cow", "rabbit" }, houses = { "chicken_coop", "barn", "hutch" }, outputs = { "egg", "milk", "wool" };
            int[] animalCosts = { 150, 400, 300 }, intervals = { 120, 180, 240 };
            db.animals = animals.Select((id, i) => Definition<AnimalBase>(id, d => { d.id = id; d.housing = houses[i]; d.product = outputs[i]; d.feed = id == "rabbit" ? "carrot" : "wheat"; d.cost = animalCosts[i]; d.interval = intervals[i]; })).ToArray();
            db.workers = new[] { "luna", "bento", "flora", "nico" }.Select((id, i) => Definition<WorkerData>(id, d => { d.id = id; d.personality = new[] { "cheerful", "careful", "messy", "grumpy" }[i]; d.specialty = (WorkerSpecialty)i; d.speed = i == 1 ? .85f : i == 2 ? 1.15f : 1; })).ToArray();
            db.regions = new[] { "riverside", "hillside", "forest", "meadow" }.Select(id => Definition<RegionData>(id, d => { d.id = id; })).ToArray();
            db.achievements = new[] { "first_harvest", "merchant", "neighbor", "rancher", "explorer" }.Select((id, i) => Definition<AchievementData>(id, d => { d.id = id; d.metric = new[] { "harvest", "sales", "orders", "animals", "expansions" }[i]; d.target = new[] { 1, 20, 1, 3, 1 }[i]; })).ToArray();
            db.villagers = new[] { "rosa", "tomas", "iris" }.Select(id => Definition<VillagerData>(id, d => { d.id = id; d.greetingKey = "greeting_" + id; })).ToArray();
            Save(db, Root + "GameDatabase.asset");
        }
        static Sprite[] Slice(string path, string[] names, Rect[] rectangles)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 150; importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;
            #pragma warning disable 0618
            importer.spritesheet = names.Select((name, i) => new SpriteMetaData { name = name, rect = rectangles[i], pivot = new Vector2(.5f, .5f), alignment = 0 }).ToArray();
            #pragma warning restore 0618
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        }
        static Rect Cell(int width, int height, int cols, int rows, int col, int row) => new Rect(col * width / (float)cols + 2, height - (row + 1) * height / (float)rows + 2, width / (float)cols - 4, height / (float)rows - 4);
        static void Map(string key, Sprite sprite) { art.Add(new SpriteLibrary.Entry { name = key, sprite = sprite }); }
        static void BuildArt()
        {
            art.Clear(); missing.Clear();
            // A generated neutral fallback is used exclusively for missing assets, never as a replacement for supplied art.
            var fallbackTexture = new Texture2D(32, 32); var pixels = Enumerable.Repeat(new Color(.75f, .55f, .8f), 1024).ToArray(); fallbackTexture.SetPixels(pixels); fallbackTexture.Apply();
            File.WriteAllBytes("Assets/Art/fallback.png", fallbackTexture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(fallbackTexture); AssetDatabase.Refresh();
            var fi = (TextureImporter)AssetImporter.GetAtPath("Assets/Art/fallback.png"); fi.textureType = TextureImporterType.Sprite; fi.spritePixelsPerUnit = 32; fi.SaveAndReimport();
            var library = ScriptableObject.CreateInstance<SpriteLibrary>(); library.fallback = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/fallback.png");
            foreach (string file in Directory.GetFiles("Assets/Art/Source", "*.jpeg"))
            {
                string id = Path.GetFileNameWithoutExtension(file); string transparent = "Assets/Art/Transparent/" + id + ".png";
                string path = File.Exists(transparent) ? transparent : file.Replace('\\', '/');
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); int w = texture.width, h = texture.height;
                var names = new List<string>(); var rectangles = new List<Rect>();
                void Add(string name, Rect rect) { names.Add(name); rectangles.Add(rect); }
                if (new[] { "wheat", "lettuce", "apple_tree", "carrot" }.Contains(id))
                {
                    string crop = id == "apple_tree" ? "apple" : id;
                    int cols = id == "carrot" ? 6 : 4, rows = id == "carrot" ? 8 : 4;
                    int[] cells = id == "carrot" ? new[] { 1, 3, 12, 17, 24 } : new[] { 0, 1, 3, 4, 6 };
                    string[] stages = { "planted", "sprout", "growing", "grown", "ready" };
                    for (int i = 0; i < 5; i++) Add("crop_" + crop + "_" + stages[i] + "_01", Cell(w, h, cols, rows, cells[i] % cols, cells[i] / cols));
                }
                else if (new[] { "chicken", "cow", "rabbit", "pig", "my_farmer" }.Contains(id))
                {
                    string prefix = id == "my_farmer" ? "player" : "animal_" + id;
                    string[] dirs = { "down", "up", "left", "right" };
                    for (int d = 0; d < 4; d++)
                    {
                        Add(prefix + "_idle_" + dirs[d], Cell(w, h, 6, 8, 0, d == 0 ? 0 : d + 1));
                        for (int f = 0; f < 4; f++) Add(prefix + "_walk_" + dirs[d] + "_" + (f + 1).ToString("00"), Cell(w, h, 6, 8, f, d + 1));
                        Add(prefix + "_walk_" + dirs[d], Cell(w, h, 6, 8, 1, d + 1));
                        Add(prefix + "_sleep_" + dirs[d], Cell(w, h, 4, 8, 2, 5));
                        Add(prefix + "_eat_" + dirs[d], Cell(w, h, 4, 8, 0, 5));
                        Add(prefix + "_rest_" + dirs[d], Cell(w, h, 4, 8, 0, 5));
                        Add(prefix + "_funny_" + dirs[d], Cell(w, h, 4, 8, 1, 6));
                    }
                }
                else if (new[] { "barn", "chicken-coop", "farmhouse", "silo", "water-well", "windmill" }.Contains(id))
                {
                    string b = id.Replace('-', '_'); if (b == "windmill") b = "mill";
                    Add("building_" + b + "_day", Cell(w, h, 2, 1, 0, 0)); Add("building_" + b + "_night", Cell(w, h, 2, 1, 1, 0));
                }
                else if (id == "tileset")
                {
                    Add("tile_grass_01", Cell(w, h, 8, 6, 0, 0)); Add("tile_soil_01", Cell(w, h, 8, 6, 3, 0)); Add("tile_water_01", Cell(w, h, 8, 6, 0, 2)); Add("tile_stone_01", Cell(w, h, 8, 6, 4, 4));
                }
                else if (id == "all-assets")
                {
                    float sx = w / 1376f, sy = h / 768f;
                    void Piece(string key, float x, float y, float width, float height) => Add(key, new Rect(x * sx, h - (y + height) * sy, width * sx, height * sy));
                    Piece("logo", 26, 50, 294, 139);
                    string[] actions = { "plant", "water", "harvest", "sell", "build", "remove", "upgrade", "inventory" };
                    for (int i = 0; i < actions.Length; i++) Piece("icon_" + actions[i], 708 + i % 4 * 52, 73 + i / 4 * 64, 46, 46);
                    string[] goods = { "wheat", "carrot", "apple", "lettuce", "chicken", "cow", "rabbit", "egg", "milk", "wool" };
                    for (int i = 0; i < goods.Length; i++) Piece("icon_" + goods[i], 925 + i % 5 * 50, 73 + i / 5 * 64, 45, 45);
                    string[] products = { "flour", "bread", "cheese", "jam" };
                    for (int i = 0; i < 4; i++) Piece("icon_" + products[i], 709 + i * 51, 243, 44, 44);
                    Piece("icon_coin", 1200, 73, 47, 47); Piece("icon_gem", 1251, 73, 47, 47);
                    string[] ui = { "home", "shop", "settings", "profile" }; for (int i = 0; i < 4; i++) Piece("icon_" + ui[i], 1180 + i * 48, 243, 42, 42);
                    Piece("btn_primary_normal", 36, 538, 87, 37); Piece("btn_primary_pressed", 140, 538, 87, 37); Piece("icon_lock", 379, 680, 58, 62);
                }
                else Add(id == "bright-sky" ? "bg_sky_day" : id == "night_sky" ? "bg_sky_night" : id == "afternoon-sky" ? "bg_sky_dusk" : id, new Rect(0, 0, w, h));
                var sprites = Slice(path, names.ToArray(), rectangles.ToArray());
                foreach (var sprite in sprites) Map(sprite.name, sprite);
                if (sprites.Length > 0) Map(id, sprites[0]);
            }
            foreach (string b in new[] { "hutch", "bakery", "dairy", "kitchen", "market_stall", "water_wheel", "storage_shed", "wood_workshop", "fence" })
                foreach (string time in new[] { "day", "night" }) Require("building_" + b + "_" + time, library.fallback);
            foreach (string icon in new[] { "workers", "orders", "expansion", "achievement", "notifications", "feed", "collect", "move" }) Require("icon_" + icon, library.fallback);
            library.entries = art.ToArray(); Save(library, Root + "SpriteLibrary.asset");
            foreach (string group in new[] { "Crops", "Animals", "UI" })
            {
                string prefix = group == "Crops" ? "crop_" : group == "Animals" ? "animal_" : "icon_";
                var atlas = new SpriteAtlas(); SpriteAtlasExtensions.Add(atlas, art.Where(e => e.name.StartsWith(prefix)).Select(e => (UnityEngine.Object)e.sprite).Distinct().ToArray());
                SpriteAtlasExtensions.SetPackingSettings(atlas, new SpriteAtlasPackingSettings { padding = 4, enableRotation = false, enableTightPacking = false });
                string path = "Assets/Art/Atlases/" + group + ".spriteatlas";
                if (!File.Exists(path)) AssetDatabase.CreateAsset(atlas, path); else { var existing = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path); EditorUtility.CopySerialized(atlas, existing); EditorUtility.SetDirty(existing); }
            }
            var material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
            string matPath = Root + "FarmSprite.mat"; var old = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (old == null) AssetDatabase.CreateAsset(material, matPath); else { EditorUtility.CopySerialized(material, old); EditorUtility.SetDirty(old); }
            File.WriteAllText("MISSING_ASSETS.md", "# Missing assets\n\nThe 21 supplied JPEG sources are preserved without renames. Transparent PNG derivatives remove only edge-connected green backgrounds. Sprite names are imported subasset aliases.\n\n## Fallback sprites\n\n" + string.Join("\n", missing.Select(x => "- `" + x + ".png`")) + "\n\n## Audio (silence until supplied)\n\n" + string.Join("\n", Enum.GetNames(typeof(SfxEvent)).Select(x => "- `sfx_" + x.ToLowerInvariant() + ".wav`")) + "\n- `music_day.ogg`\n- `music_night.ogg`\n\nClean production exports with native alpha are still desirable; the supplied JPEG sheets have compression edges and irregular frame spacing. Sky and terrain backgrounds are intentionally opaque.\n");
        }
        static void Require(string key, Sprite fallback)
        {
            if (art.Exists(x => x.name == key)) return;
            // TODO: supply the exact PNG names written by Require to MISSING_ASSETS.md.
            missing.Add(key); Map(key, null);
        }
        static void BuildScenes()
        {
            foreach (string name in new[] { "Boot", "Title", "RegionSelect", "Farm" })
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); camera.tag = "MainCamera";
                var cam = camera.GetComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 7; cam.backgroundColor = new Color(.42f, .66f, .37f);
                camera.transform.position = new Vector3(10, 10, -10); camera.AddComponent<UniversalAdditionalCameraData>();
                var light = new GameObject("Global Light 2D").AddComponent<Light2D>(); light.lightType = Light2D.LightType.Global; light.intensity = 1;
                EditorSceneManager.SaveScene(scene, "Assets/Scenes/" + name + ".unity");
            }
            EditorBuildSettings.scenes = new[] { "Boot", "Title", "RegionSelect", "Farm" }.Select(n => new EditorBuildSettingsScene("Assets/Scenes/" + n + ".unity", true)).ToArray();
            EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
        }
        static void BuildPrefabs()
        {
            foreach (string name in new[] { "Player", "Crop", "Animal", "Building", "Worker" })
            {
                var go = new GameObject(name, typeof(SpriteRenderer)); go.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<SpriteLibrary>(Root + "SpriteLibrary.asset").fallback;
                PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/" + name + ".prefab"); UnityEngine.Object.DestroyImmediate(go);
            }
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasGroup), typeof(PanelBase));
            PrefabUtility.SaveAsPrefabAsset(panel, "Assets/Prefabs/Panel.prefab"); UnityEngine.Object.DestroyImmediate(panel);
        }
        public static void Verify()
        {
            if (EditorUtility.scriptCompilationFailed) throw new Exception("Script compilation failed.");
            if (AssetDatabase.LoadAssetAtPath<GameDatabase>(Root + "GameDatabase.asset") == null) throw new Exception("Database missing.");
            foreach (var scene in EditorBuildSettings.scenes) if (!File.Exists(scene.path)) throw new Exception("Missing scene " + scene.path);
            UnityEngine.Debug.Log("BROTINHO_VERIFY_OK");
        }
    }
}
