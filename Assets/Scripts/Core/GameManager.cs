using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Brotinho
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameSession Game { get; private set; }
        public Localization Text { get; private set; }
        public SpriteLibrary Art { get; private set; }
        public OfflineReport Offline { get; private set; }
        public string SelectedCrop = "lettuce";
        public PlayerTool Tool = PlayerTool.Plant;
        public string Placement;
        public int Rotation;
        public Vector2Int PlacementTile = new Vector2Int(8, 8);
        public WorkerInstance ZoneWorker;
        public PlayerMovement Farmer { get; private set; }
        double accumulator;
        SaveSystem saves;
        double autosave;
        bool suspended, testMode;
        bool inFarm;
        public string SaveError => saves.LastError;
        public bool IsTestSession => testMode;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartGame()
        {
            if (Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
        }
        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60; Application.runInBackground = true;
            Art = Resources.Load<SpriteLibrary>("SpriteLibrary");
            testMode = Environment.GetEnvironmentVariable("BROTINHO_TEST_MODE") == "1" || Application.isBatchMode && Array.Exists(Environment.GetCommandLineArgs(), x => x == "-runTests");
            var data = Resources.Load<GameDatabase>("GameDatabase");
            saves = new SaveSystem(Application.persistentDataPath, state => SaveStateValidator.ForDatabase(state, data));
            Game = new GameSession(testMode ? GameState.New() : saves.Load(), data);
            Text = new Localization(Game.State);
            CatchUp();
            gameObject.AddComponent<AudioManager>().Initialize(this);
            SceneManager.sceneLoaded += SceneReady;
            Game.Bus.Changed += AdvanceTutorial;
            SceneReady(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        void SceneReady(Scene scene, LoadSceneMode mode)
        {
            Placement = null;
            inFarm = scene.name == "Farm";
            var ui = new GameObject("Interface").AddComponent<GameUI>();
            ui.Initialize(this, scene.name);
            if (scene.name == "Boot") { StartCoroutine(LeaveBoot()); return; }
            if (saves.LastError != null) { Game.Bus.Notify(saves.LastError); Game.Bus.Open("error"); }
            if (scene.name == "Farm")
            {
                new GameObject("FarmWorld").AddComponent<FarmView>().Initialize(this);
                new GameObject("FeedbackPool").AddComponent<FeedbackPool>().Initialize(this);
                Camera.main.gameObject.AddComponent<PlayerController>().Initialize(this);
                var prefab = Resources.Load<GameObject>("Prefabs/Player");
                var farmer = prefab != null ? Instantiate(prefab) : new GameObject("Farmer", typeof(SpriteRenderer));
                Farmer = farmer.GetComponent<PlayerMovement>() ?? farmer.AddComponent<PlayerMovement>(); Farmer.Initialize(this);
                var light = FindAnyObjectByType<UnityEngine.Rendering.Universal.Light2D>(); if (light != null) light.gameObject.AddComponent<DayNightCycle>();
                if (Offline != null && Offline.seconds > 2) Game.Bus.Open("afk");
            }
        }
        System.Collections.IEnumerator LeaveBoot() { yield return null; yield return new WaitForSecondsRealtime(.2f); SceneLoader.Load("Title"); }
        void Update()
        {
            if (suspended || !inFarm) return;
            accumulator += Time.unscaledDeltaTime;
            if (accumulator >= 1) { var seconds = (int)accumulator; accumulator -= seconds; Simulation.Advance(Game, seconds); Game.Bus.Refresh(); }
            autosave += Time.unscaledDeltaTime; if (autosave >= 30) { autosave = 0; SaveNow(); }
        }
        public void Play() => SceneLoader.Load(string.IsNullOrEmpty(Game.State.region) ? "RegionSelect" : "Farm");
        public void ChooseRegion(string id) { if (Game.State.region.Length > 0) return; Game.State.region = id; SaveNow(); SceneLoader.Load("Farm"); }
        void CatchUp()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            double seconds = TimeManager.OfflineSeconds(Game.State.savedUtc, now, Game.Data.offlineCapSeconds);
            bool silent = Game.Bus.Silent; Game.Bus.Silent = true;
            try { Offline = Simulation.Advance(Game, seconds); } finally { Game.Bus.Silent = silent; }
            Game.State.savedUtc = now; SaveNow();
        }
        public void SaveNow()
        {
            if (Game == null || testMode) return;
            if (accumulator > 0) { Simulation.Advance(Game, accumulator); accumulator = 0; }
            if (!saves.Save(Game.State, DateTimeOffset.UtcNow.ToUnixTimeSeconds())) { Game.Bus.Notify(saves.LastError ?? "save_failed"); Game.Bus.Open("error"); }
        }
        public void ResetGame()
        {
            if (!testMode) saves.Reset();
            Game = new GameSession(GameState.New(), Game.Data, Game.Bus); Text = new Localization(Game.State);
            Offline = null; accumulator = autosave = 0; Placement = null; ZoneWorker = null; Tool = PlayerTool.Plant; SelectedCrop = "lettuce";
            SaveNow(); SceneLoader.Load("Title");
        }
        void OnApplicationPause(bool paused)
        {
            if (Game == null) return;
            if (paused) { if (!suspended) SaveNow(); suspended = true; }
            else if (suspended) { suspended = false; CatchUp(); Game.Bus.Refresh(); if (Offline.seconds > 2) Game.Bus.Open("afk"); }
        }
        void OnApplicationQuit() => SaveNow();
        void AdvanceTutorial() => TutorialOverlay.Advance(Game.State);
        void OnDestroy() { if (Instance != this) return; SceneManager.sceneLoaded -= SceneReady; Instance = null; }
    }
}
