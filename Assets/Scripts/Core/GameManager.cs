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
        double accumulator;
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
            Game = new GameSession(GameState.New(), Resources.Load<GameDatabase>("GameDatabase"));
            Text = new Localization(Game.State);
            SceneManager.sceneLoaded += SceneReady;
            Game.Bus.Changed += AdvanceTutorial;
            SceneReady(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        void SceneReady(Scene scene, LoadSceneMode mode)
        {
            Placement = null;
            if (scene.name == "Boot") { SceneLoader.Load("Title"); return; }
            var ui = new GameObject("Interface").AddComponent<GameUI>();
            ui.Initialize(this, scene.name);
            if (scene.name == "Farm")
            {
                new GameObject("FarmWorld").AddComponent<FarmView>().Initialize(this);
                Camera.main.gameObject.AddComponent<PlayerController>().Initialize(this);
            }
        }
        void Update()
        {
            if (SceneManager.GetActiveScene().name != "Farm") return;
            accumulator += Time.unscaledDeltaTime;
            if (accumulator >= 1) { var seconds = (int)accumulator; accumulator -= seconds; Simulation.Advance(Game, seconds); Game.Bus.Refresh(); }
        }
        public void Play() => SceneLoader.Load(string.IsNullOrEmpty(Game.State.region) ? "RegionSelect" : "Farm");
        public void ChooseRegion(string id) { if (Game.State.region.Length > 0) return; Game.State.region = id; SceneLoader.Load("Farm"); }
        void AdvanceTutorial() { }
        void OnDestroy() { if (Instance != this) return; SceneManager.sceneLoaded -= SceneReady; Instance = null; }
    }
}
