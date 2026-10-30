using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace Brotinho
{
    // A responsive uGUI shell. Feature panels are populated from the same data used by the simulation.
    public partial class GameUI : MonoBehaviour
    {
        public static bool ModalOpen { get; private set; }
        GameManager manager;
        RectTransform safe, top, bottom, modal, content;
        TextMeshProUGUI hud, toast;
        string scene, currentPanel;
        float toastUntil;
        TMP_FontAsset font;
        public void Initialize(GameManager manager, string scene)
        {
            this.manager = manager; this.scene = scene; ModalOpen = false;
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            if (FindAnyObjectByType<EventSystem>() == null) { var e = new GameObject("EventSystem"); e.AddComponent<EventSystem>(); e.AddComponent<InputSystemUIInputModule>(); }
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            safe = Rect("SafeArea", transform); Stretch(safe); safe.gameObject.AddComponent<SafeArea>();
            top = Box(safe, new Vector2(0, .85f), Vector2.one, new Color(.98f, .94f, .79f, .98f)); Layout(top);
            bottom = Box(safe, Vector2.zero, new Vector2(1, .20f), new Color(.98f, .94f, .79f, .98f)); Layout(bottom);
            if (scene == "Farm") FarmHUD(); else MainMenu();
            toast = Label(safe, "", 32); toast.rectTransform.anchorMin = new Vector2(.05f, .72f); toast.rectTransform.anchorMax = new Vector2(.95f, .80f);
            toast.rectTransform.offsetMin = toast.rectTransform.offsetMax = Vector2.zero; toast.alignment = TextAlignmentOptions.Center;
            manager.Game.Bus.Toast += ShowToast; manager.Game.Bus.Changed += Refresh; manager.Game.Bus.OpenPanel += Open;
            Refresh();
        }
        string L(string key) => manager.Text.Get(key);
        public static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); return rect;
        }
        static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        RectTransform Box(Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var rect = Rect("Panel", parent); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            rect.gameObject.AddComponent<UnityEngine.UI.Image>().color = color; rect.gameObject.AddComponent<PanelBase>(); return rect;
        }
        void Layout(RectTransform rect)
        {
            var layout = rect.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); layout.padding = new RectOffset(24, 24, 14, 14); layout.spacing = 12;
            layout.childControlHeight = true; layout.childControlWidth = true; layout.childForceExpandHeight = false;
        }
        TextMeshProUGUI Label(Transform parent, string text, int size = 34)
        {
            var rect = Rect("Text", parent); var label = rect.gameObject.AddComponent<TextMeshProUGUI>(); label.font = font;
            label.text = text; label.fontSize = size; label.color = new Color(.18f, .12f, .08f); label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            var le = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); le.minHeight = size + 18; le.preferredHeight = size + 24;
            return label;
        }
        void Button(Transform parent, string text, Action action)
        {
            var rect = Rect("Button", parent); rect.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.82f, .9f, .57f);
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); var layout = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); layout.minHeight = 74;
            var label = Label(rect, text, 30); Stretch(label.rectTransform); label.alignment = TextAlignmentOptions.Center;
            button.onClick.AddListener(() => { manager.Game.Bus.Play(SfxEvent.Click); action(); });
        }
        void MainMenu()
        {
            Label(top, L("game_name"), 54);
            if (scene == "RegionSelect")
            {
                var area = Box(safe, new Vector2(.06f, .23f), new Vector2(.94f, .83f), new Color(1, .98f, .9f)); Layout(area);
                foreach (var region in manager.Game.Data.regions)
                { string id = region.id; Button(area, L("region_" + id), () => manager.ChooseRegion(id)); Label(area, L("trait_" + id), 26); }
            }
            else { Button(bottom, L("play"), manager.Play); Button(bottom, L("continue"), manager.Play); }
        }
        void FarmHUD()
        {
            hud = Label(top, "", 32);
            Button(top, L("orders"), () => Open("orders"));
            var cropRow = QuickRow(bottom);
            foreach (var crop in manager.Game.Data.crops)
            { var id = crop.id; Button(cropRow, L("crop_" + id), () => { manager.SelectedCrop = id; manager.Tool = PlayerTool.Plant; ShowToast("plant"); }); }
            var toolRow = QuickRow(bottom);
            Button(toolRow, L("water"), () => manager.Tool = PlayerTool.Water); Button(toolRow, L("harvest"), () => manager.Tool = PlayerTool.Harvest);
            var menus = QuickRow(bottom); foreach (string id in new[] { "shop", "inventory", "sell" }) { string target = id; Button(menus, L(id), () => Open(target)); }
        }
        RectTransform QuickRow(Transform parent)
        {
            var rect = Rect("Actions", parent); var layout = rect.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>(); layout.spacing = 8; layout.childControlHeight = layout.childControlWidth = true; layout.childForceExpandWidth = true;
            rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().minHeight = 78; return rect;
        }
        void Refresh()
        {
            if (hud == null) return;
            var s = manager.Game.State;
            hud.text = L("coins") + " " + s.coins + "   " + L("gems") + " " + s.gems + "\n" + L("day") + " " + (TimeManager.Day(s.elapsed) + 1) + " · " + TimeManager.Hour(s.elapsed).ToString("00") + ":00 · " + L("weather_" + WeatherSystem.At(s.elapsed).ToString().ToLowerInvariant());
        }
        public void Open(string panel)
        {
            if (modal != null) Destroy(modal.gameObject); currentPanel = panel; ModalOpen = true;
            modal = Box(safe, new Vector2(.06f, .22f), new Vector2(.94f, .84f), new Color(1, .97f, .86f)); Layout(modal);
            Label(modal, L(panel), 38); Button(modal, L("close"), () => { Destroy(modal.gameObject); ModalOpen = false; });
            var game = manager.Game;
            if (panel == "shop") foreach (var crop in game.Data.crops) { string id = crop.id; Button(modal, L("crop_" + id) + " · " + crop.seedCost, () => ShopManager.BuySeeds(game, id)); }
            if (panel == "inventory" || panel == "sell") foreach (var product in game.Data.products)
            {
                if (game.State.Count(product.id) == 0) continue; string id = product.id;
                Button(modal, L(game.Data.Crop(id) != null ? "crop_" + id : "product_" + id) + " ×" + game.State.Count(id) + " · " + L("sell_all") + " " + PriceTable.Quote(game, id, game.State.Count(id)), () => { SellManager.Sell(game, id, game.State.Count(id)); Open(panel); });
            }
            if (panel == "orders") foreach (var order in game.State.orders) { string id = order.id; Button(modal, L("crop_" + order.product) + " ×" + order.quantity + " → " + order.reward + " " + L("coins"), () => { OrderManager.Complete(game, id); Open(panel); }); }
            modal.GetComponent<PanelBase>().Open();
        }
        void ShowToast(string key) { if (toast == null) return; toast.text = L(key); toastUntil = Time.unscaledTime + 2.5f; }
        void Update() { if (toast != null && Time.unscaledTime > toastUntil) toast.text = ""; }
        void OnDestroy()
        {
            ModalOpen = false; if (manager == null) return;
            manager.Game.Bus.Toast -= ShowToast; manager.Game.Bus.Changed -= Refresh; manager.Game.Bus.OpenPanel -= Open;
        }
    }
}
