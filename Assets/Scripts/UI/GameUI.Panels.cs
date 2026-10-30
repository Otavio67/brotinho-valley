using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Brotinho
{
    public partial class GameUI
    {
        readonly List<Action> bindings = new List<Action>();
        WorkerInstance selectedWorker;
        BuildingInstance selectedBuilding;
        int selectedExpansion;
        void Icon(Transform parent, string key, float height = 72)
        {
            var rect = Rect("Icon_" + key, parent); var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.sprite = manager.Art.Get(key); image.preserveAspect = true; image.raycastTarget = false;
            var layout = rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); layout.minHeight = height; layout.preferredHeight = height; layout.preferredWidth = height;
        }
        public static void RequestRemove(GameManager manager, int x, int y)
        {
            var ui = UnityEngine.Object.FindAnyObjectByType<GameUI>(); if (ui != null) ui.Confirm("remove", () => manager.Game.Remove(x, y), null);
        }
        void Bind(Transform parent, Func<string> value, int size = 30)
        {
            var label = Label(parent, value(), size); bindings.Add(() => { if (label != null) label.text = value(); });
        }
        RectTransform Row(Transform parent)
        {
            var rect = Rect("Row", parent); var group = rect.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            group.spacing = 8; group.childControlHeight = true; group.childControlWidth = true; group.childForceExpandWidth = true;
            rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().minHeight = 78; return rect;
        }
        RectTransform Cards(Transform parent, int count, float height)
        {
            var rect = Rect("CardGrid", parent); var grid = rect.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>(); grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 2; grid.cellSize = new Vector2(440, height); grid.spacing = new Vector2(12, 12);
            rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().minHeight = Mathf.CeilToInt(count / 2f) * (height + 12);
            rect.gameObject.AddComponent<ResponsiveGrid>(); return rect;
        }
        RectTransform Card(Transform parent) { var rect = Rect("Card", parent); Layout(rect); return rect; }
        RectTransform Scroll(Transform parent)
        {
            var rect = Rect("Scroll", parent); rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(1, .86f); rect.offsetMin = new Vector2(12, 12); rect.offsetMax = new Vector2(-12, -12);
            var scroll = rect.gameObject.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = false;
            var viewport = Rect("Viewport", rect); Stretch(viewport); viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, .02f); viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
            var body = Rect("Content", viewport); body.anchorMin = new Vector2(0, 1); body.anchorMax = Vector2.one; body.pivot = new Vector2(.5f, 1); body.sizeDelta = Vector2.zero;
            Layout(body); var fit = body.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>(); fit.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = body; return body;
        }
        void ClosePanel()
        {
            bindings.Clear(); currentPanel = null; ModalOpen = false;
            if (modal != null) { var closing = modal; closing.GetComponent<PanelBase>().Close(); Destroy(closing.gameObject, .2f); modal = null; }
        }
        string ItemName(string id) => id.StartsWith("seed_") ? L("seed") + " " + L("crop_" + id.Substring(5)) : manager.Game.Data.Crop(id) != null ? L("crop_" + id) : L("product_" + id);
        void ShowPanel(string panel)
        {
            ClosePanel(); currentPanel = panel; ModalOpen = true;
            modal = Box(safe, new Vector2(.035f, .07f), new Vector2(.965f, .93f), new Color(1, .97f, .86f, .995f)); modal.name = "Panel_" + panel;
            var header = Rect("Header", modal); header.anchorMin = new Vector2(0, .87f); header.anchorMax = Vector2.one; header.offsetMin = new Vector2(24, 8); header.offsetMax = new Vector2(-24, -8); Layout(header);
            Label(header, L(panel), 40); Button(header, L("close"), ClosePanel); content = Scroll(modal);
            Populate(panel); modal.GetComponent<PanelBase>().Open();
        }
        void Populate(string panel)
        {
            var game = manager.Game; var s = game.State;
            switch (panel)
            {
                case "build":
                    Label(content, L("build_hint"), 28);
                    foreach (string category in new[] { "buildings", "decorations" })
                    {
                        Label(content, L(category), 34);
                        var available = Array.FindAll(game.Data.buildings, d => d.decoration == (category == "decorations")); var buildingGrid = Cards(content, available.Length, 225);
                        foreach (var d in available)
                        {
                            var data = d; var card = Card(buildingGrid); Icon(card, "building_" + d.id + "_day", 90);
                            Button(card, L("building_" + d.id) + " · " + ConstructionService.Cost(game, d) + " " + L("coins") + (ConstructionService.Unlocked(game, d) ? "" : " · " + L("locked")), () => { manager.Placement = data.id; manager.Rotation = 0; ClosePanel(); });
                        }
                    }
                    Button(content, L("undo"), () => ConstructionService.Undo(game)); break;
                case "shop":
                    var tabs = Row(content); foreach (string tab in new[] { "seeds", "animals", "build", "expansion", "storage" }) { string id = tab; Button(tabs, L(id), () => ShowPanel(id == "seeds" ? "shop" : id)); }
                    var seedGrid = Cards(content, game.Data.crops.Length, 210);
                    foreach (var d in game.Data.crops) { var data = d; var card = Card(seedGrid); Icon(card, "icon_" + d.id, 65); Button(card, L("seed") + " " + L("crop_" + d.id) + " · " + d.seedCost + " " + L("coins"), () => Confirm("buy", () => ShopManager.BuySeeds(game, data.id), "shop")); } break;
                case "storage": Bind(content, () => L("capacity") + " " + s.Capacity + " / " + L("per_type")); Button(content, L("building_storage_shed") + " · 150", () => { manager.Placement = "storage_shed"; ClosePanel(); }); break;
                case "inventory":
                    Bind(content, () => L("capacity") + " " + s.Capacity + " / " + L("per_type"));
                    bool any = false; var stacks = s.inventory.FindAll(x => x.count > 0); var itemGrid = Cards(content, stacks.Count, 300);
                    foreach (var stack in s.inventory)
                    {
                        if (stack.count == 0) continue; any = true; var item = stack;
                        var card = Card(itemGrid); Icon(card, "icon_" + (item.id.StartsWith("seed_") ? item.id.Substring(5) : item.id), 54);
                        Bind(card, () => ItemName(item.id) + " · " + item.count + " / " + s.Capacity);
                        Progress(card, () => item.count / (float)s.Capacity);
                        if (item.id.StartsWith("seed_")) Button(card, L("use"), () => { manager.SelectedCrop = item.id.Substring(5); manager.Tool = PlayerTool.Plant; ClosePanel(); });
                        else { var row = Row(card); Button(row, L("sell_one"), () => SellManager.Sell(game, item.id, 1)); Button(row, L("sell_all"), () => SellManager.Sell(game, item.id, s.Count(item.id))); }
                    }
                    if (!any) Label(content, L("empty_inventory")); break;
                case "sell":
                    foreach (var product in game.Data.products)
                    {
                        var p = product; Bind(content, () => ItemName(p.id) + " · " + s.Count(p.id) + " · " + PriceTable.Unit(game, p.id) + " " + L("coins") + " · " + PriceTable.DailyMultiplier(p.id, TimeManager.Day(s.elapsed)).ToString("0.00") + "×");
                        var row = Row(content); Button(row, L("sell_one"), () => SellManager.Sell(game, p.id, 1)); Button(row, L("sell_all"), () => SellManager.Sell(game, p.id, s.Count(p.id)));
                    } break;
                case "workers":
                    Bind(content, () => L("worker_slots") + " " + s.workers.Count + "/" + LevelSystem.WorkerSlots(s));
                    Button(content, L("hire") + " · " + WorkerAI.HireCost(s.workers.Count) + " " + L("coins"), () => Confirm("hire", () => WorkerAI.Hire(game), "workers"));
                    if (s.workers.Count == 0) Label(content, L("empty_workers"));
                    foreach (var worker in s.workers)
                    {
                        Icon(content, "player_idle_down", 80);
                        var w = worker; Bind(content, () => L("worker_" + w.definition) + " · " + L("task_" + w.task.ToString().ToLowerInvariant()) + " · " + (int)w.energy + "%"); Progress(content, () => (float)w.energy / 100);
                        Button(content, L("details"), () => { selectedWorker = w; ShowPanel("worker_detail"); });
                    } break;
                case "worker_detail":
                    if (selectedWorker == null) { Label(content, L("empty_workers")); break; }
                    var wd = Array.Find(game.Data.workers, x => x.id == selectedWorker.definition);
                    Label(content, L("worker_" + wd.id) + " · " + L("personality_" + wd.personality));
                    Label(content, L("specialty_" + wd.specialty.ToString().ToLowerInvariant())); Progress(content, () => (float)selectedWorker.energy / 100);
                    Dropdown(content, new[] { L("task_rest"), L("task_farm"), L("task_animals"), L("task_process"), L("task_sell") }, (int)selectedWorker.task, v => { selectedWorker.task = (WorkerTask)v; game.Bus.Refresh(); });
                    Button(content, L("assign_zone"), () => { manager.ZoneWorker = selectedWorker; ClosePanel(); ShowToast("zone_hint"); });
                    Bind(content, () => L("zone") + " " + selectedWorker.x + "," + selectedWorker.y + " · " + selectedWorker.width + "×" + selectedWorker.height);
                    foreach (var crop in game.Data.crops) { string id = crop.id; Button(content, L("crop_" + id), () => selectedWorker.crop = id); }
                    Button(content, L("rest"), () => { selectedWorker.task = WorkerTask.Rest; game.Bus.Refresh(); });
                    Bind(content, () => L("history") + ": " + string.Join(", ", selectedWorker.history.ConvertAll(L))); break;
                case "animals":
                    foreach (var animal in game.Data.animals) { string id = animal.id; Button(content, L("animal_" + id) + " · " + animal.cost + " " + L("coins"), () => Confirm("buy", () => ProductionController.Buy(game, id), "animals")); }
                    if (s.animals.Count == 0) Label(content, L("empty_animals"));
                    foreach (var animal in s.animals)
                    {
                        var a = animal; Bind(content, () => L("animal_" + a.definition) + " · " + L("feed") + " " + a.feed + " · " + L("ready") + " " + a.ready);
                        Progress(content, () => (float)(a.progress / game.Data.Animal(a.definition).interval));
                        var row = Row(content); Button(row, L("feed"), () => ProductionController.Feed(game, a)); Button(row, L("collect"), () => ProductionController.Collect(game, a));
                    } break;
                case "processing":
                    foreach (var b in s.buildings) if (game.Data.Building(b.definition).processSeconds > 0) { var building = b; Button(content, L("building_" + b.definition), () => { selectedBuilding = building; ShowPanel("processor"); }); }
                    if (!s.buildings.Exists(b => game.Data.Building(b.definition).processSeconds > 0)) Label(content, L("empty_processing")); break;
                case "processor":
                    if (selectedBuilding == null) break; var bdata = game.Data.Building(selectedBuilding.definition);
                    Label(content, L("building_" + bdata.id));
                    foreach (var input in bdata.inputs) Label(content, ItemName(input.id) + " ×" + input.count);
                    Label(content, "→ " + ItemName(bdata.output) + " · " + bdata.processSeconds + " " + L("seconds"));
                    Bind(content, () => L("queue") + " " + selectedBuilding.queue + "/5 · " + L("output") + " " + selectedBuilding.output);
                    Progress(content, () => (float)(selectedBuilding.progress / bdata.processSeconds));
                    Button(content, L("enqueue"), () => ProcessingBuilding.Enqueue(game, selectedBuilding)); Button(content, L("collect"), () => ProcessingBuilding.Collect(game, selectedBuilding)); break;
                case "orders":
                    foreach (var order in s.orders)
                    {
                        var o = order; Label(content, L("villager_" + o.villager) + " · " + ItemName(o.product) + " ×" + o.quantity);
                        Bind(content, () => Math.Max(0, (int)(o.deadline - s.elapsed)) + " " + L("seconds") + " · " + o.reward + " " + L("coins") + " · " + o.xp + " " + L("xp") + " · " + o.gems + " " + L("gems"));
                        Button(content, L("complete"), () => { OrderManager.Complete(game, o.id); ShowPanel("orders"); });
                    }
                    if (s.orders.Count == 0) Label(content, L("empty_orders")); break;
                case "expansion":
                    var blocks = ExpansionManager.Available(game); Label(content, L("cost") + " " + ExpansionManager.Cost(s.expansions));
                    if (blocks.Count == 0) { Label(content, L("no_expansion")); break; }
                    selectedExpansion = Mathf.Clamp(selectedExpansion, 0, blocks.Count - 1); var block = blocks[selectedExpansion];
                    Label(content, L("next_block") + " " + block.x + "," + block.y + " · 4×4");
                    ExpansionPreview(content, block); Button(content, L("next"), () => { selectedExpansion = (selectedExpansion + 1) % blocks.Count; ShowPanel("expansion"); });
                    Button(content, L("unlock"), () => Confirm("unlock", () => ExpansionManager.Unlock(game, block.x, block.y), "expansion")); break;
                case "achievements":
                    foreach (var achievement in game.Data.achievements)
                    {
                        var a = achievement; Bind(content, () => L("achievement_" + a.id) + " " + AchievementManager.Progress(s, a.metric) + "/" + a.target);
                        Progress(content, () => AchievementManager.Progress(s, a.metric) / (float)a.target);
                        Button(content, L("claim") + " · " + a.gems + " " + L("gems"), () => AchievementManager.Claim(game, a));
                    } break;
                case "daily":
                    Bind(content, () => L("refresh_in") + " " + (720 - (int)s.elapsed % 720) + " " + L("seconds"));
                    foreach (string task in new[] { "harvest", "sales", "orders" }) { string id = task; Bind(content, () => L("daily_" + id) + " " + AchievementManager.DailyProgress(s, id) + "/" + AchievementManager.DailyTarget(id)); Button(content, L("claim") + " · 3 " + L("gems"), () => AchievementManager.ClaimDaily(game, id)); } break;
                case "profile":
                    Icon(content, "avatar_" + (s.avatar + 1).ToString("00"), 130);
                    Input(content, s.playerName, value => s.playerName = value.Length > 24 ? value.Substring(0, 24) : value);
                    Button(content, L("avatar") + " " + (s.avatar + 1), () => { s.avatar = (s.avatar + 1) % 6; ShowPanel("profile"); });
                    Bind(content, () => L("level") + " " + LevelSystem.Level(s.xp) + " · " + s.xp + "/" + LevelSystem.NextThreshold(s.xp) + " " + L("xp"));
                    Progress(content, () => s.xp / (float)LevelSystem.NextThreshold(s.xp));
                    Bind(content, () => L("stats") + "\n" + L("harvest") + " " + s.harvested + " · " + L("sales") + " " + s.sold + " · " + L("orders") + " " + s.ordersCompleted); break;
                case "settings":
                    Button(content, L("language") + " · " + (s.language == "en" ? "English" : "Português"), () => { s.language = s.language == "en" ? "pt-BR" : "en"; SceneLoader.Load(scene); });
                    Toggle(content, "sound", () => s.sound, v => s.sound = v); Toggle(content, "music", () => s.music, v => s.music = v);
                    Toggle(content, "vibration", () => s.vibration, v => s.vibration = v); Toggle(content, "notifications", () => s.notifications, v => s.notifications = v);
                    Button(content, L("reset_save"), () => Confirm("reset_save", manager.ResetGame, null)); break;
                case "menu": case "pause":
                    Button(content, L("resume"), ClosePanel);
                    foreach (string target in new[] { "settings", "profile", "achievements", "daily", "animals", "processing", "expansion", "orders", "sell", "dialog", "credits", "debug" }) { string id = target; if (id != "debug" || UnityEngine.Debug.isDebugBuild) Button(content, L(id), () => ShowPanel(id)); }
                    Button(content, L("quit_title"), () => SceneLoader.Load("Title")); break;
                case "afk":
                    var report = manager.Offline; if (report == null) { Label(content, L("no_afk")); break; }
                    Label(content, "✓ " + L("afk_welcome")); Label(content, ((int)report.seconds / 60) + " " + L("minutes"));
                    Label(content, L("harvest") + " " + report.harvests + "\n" + L("production") + " " + report.production + "\n" + L("sales") + " " + report.sales + "\n" + L("coins") + " +" + report.coins);
                    foreach (string issue in report.issues) Label(content, L(issue)); break;
                case "dialog": Label(content, L("villager_rosa")); Label(content, L("greeting_rosa")); Button(content, L("next"), () => { ShowPanel("orders"); }); Button(content, L("maybe_later"), ClosePanel); break;
                case "credits": Label(content, L("credits_body")); break;
                case "error": Label(content, L("save_failed")); Button(content, L("retry"), manager.SaveNow); break;
                case "debug": BuildDebug(content); break;
            }
        }
        void Confirm(string title, Action action, string returnPanel)
        {
            ShowPanel("confirm"); Label(content, L(title)); Label(content, L("confirm_message"));
            Button(content, L("confirm"), () => { action(); if (returnPanel != null) ShowPanel(returnPanel); else ClosePanel(); });
            Button(content, L("cancel"), () => { if (returnPanel != null) ShowPanel(returnPanel); else ClosePanel(); });
        }
        void Progress(Transform parent, Func<float> value)
        {
            var rect = Rect("Progress", parent); rect.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.3f, .23f, .15f);
            rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().minHeight = 18;
            var fill = Rect("Fill", rect); var image = fill.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color(.4f, .68f, .26f); image.raycastTarget = false;
            Action update = () => { if (fill == null) return; fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(Mathf.Clamp01(value()), 1); fill.offsetMin = fill.offsetMax = Vector2.zero; }; update(); bindings.Add(update);
        }
        void Toggle(Transform parent, string key, Func<bool> value, Action<bool> setter)
        { Button(parent, L(key) + " · " + L(value() ? "on" : "off"), () => { setter(!value()); ShowPanel("settings"); }); }
        void Input(Transform parent, string value, Action<string> change)
        {
            var rect = Rect("Input", parent); rect.gameObject.AddComponent<UnityEngine.UI.Image>().color = Color.white; rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().minHeight = 80;
            var field = rect.gameObject.AddComponent<TMP_InputField>(); var text = Label(rect, value); Stretch(text.rectTransform);
            field.textViewport = rect; field.textComponent = text; field.characterLimit = 24; field.text = value; field.onEndEdit.AddListener(v => change(v));
        }
        void Dropdown(Transform parent, string[] options, int selected, Action<int> changed)
        {
            var rect = Rect("TaskDropdown", parent); rect.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.82f, .9f, .57f); rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().minHeight = 80;
            var dropdown = rect.gameObject.AddComponent<TMP_Dropdown>(); var caption = Label(rect, options[selected]); Stretch(caption.rectTransform); dropdown.captionText = caption;
            var template = Rect("Template", rect); template.anchorMin = Vector2.zero; template.anchorMax = new Vector2(1, 0); template.pivot = new Vector2(.5f, 1); template.sizeDelta = new Vector2(0, 360); template.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(1, .98f, .86f);
            var viewport = Rect("Viewport", template); Stretch(viewport); viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var list = Rect("Content", viewport); list.anchorMin = new Vector2(0, 1); list.anchorMax = Vector2.one; list.pivot = new Vector2(.5f, 1); list.sizeDelta = new Vector2(0, 72);
            var item = Rect("Item", list); item.anchorMin = new Vector2(0, .5f); item.anchorMax = new Vector2(1, .5f); item.sizeDelta = new Vector2(0, 72);
            var bg = item.gameObject.AddComponent<UnityEngine.UI.Image>(); bg.color = new Color(.94f, .91f, .8f);
            var toggle = item.gameObject.AddComponent<UnityEngine.UI.Toggle>(); toggle.targetGraphic = bg;
            var itemText = Label(item, options[0]); Stretch(itemText.rectTransform); dropdown.itemText = itemText;
            var scroll = template.gameObject.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = false; scroll.viewport = viewport; scroll.content = list;
            dropdown.template = template; template.gameObject.SetActive(false); dropdown.ClearOptions(); dropdown.AddOptions(new List<string>(options)); dropdown.SetValueWithoutNotify(selected); dropdown.onValueChanged.AddListener(v => changed(v));
        }
        void ExpansionPreview(Transform parent, LandBlock selected)
        {
            var rect = Rect("MapPreview", parent); rect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().minHeight = 300;
            for (int y = 0; y < 21; y++) for (int x = 0; x < 21; x++)
            {
                var cell = Rect("Cell", rect); cell.anchorMin = new Vector2(x / 21f, y / 21f); cell.anchorMax = new Vector2((x + 1) / 21f, (y + 1) / 21f); cell.offsetMin = Vector2.one; cell.offsetMax = -Vector2.one;
                bool next = x >= selected.x && x < selected.x + 4 && y >= selected.y && y < selected.y + 4;
                cell.gameObject.AddComponent<UnityEngine.UI.Image>().color = next ? new Color(1, .8f, .2f) : manager.Game.Grid.Unlocked(x, y) ? new Color(.3f, .6f, .25f) : new Color(.35f, .39f, .38f);
            }
        }
        void BuildDebug(Transform parent)
        {
            if (!DebugMenu.Enabled) return;
            Button(parent, L("add_coins"), () => DebugMenu.AddCoins(manager.Game));
            Button(parent, L("skip_time"), () => DebugMenu.SkipTime(manager.Game));
            Button(parent, L("unlock"), () => { var blocks = ExpansionManager.Available(manager.Game); if (blocks.Count > 0) ExpansionManager.Unlock(manager.Game, blocks[0].x, blocks[0].y, true); });
            Button(parent, L("reset_save"), () => Confirm("reset_save", manager.ResetGame, null));
        }
    }
}
