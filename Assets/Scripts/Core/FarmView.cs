using System.Collections.Generic;
using UnityEngine;
namespace Brotinho
{
    public sealed class FarmView : MonoBehaviour
    {
        GameManager manager;
        readonly List<SpriteRenderer> tiles = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> crops = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> buildings = new List<SpriteRenderer>();
        readonly List<AnimalBehavior> animals = new List<AnimalBehavior>();
        readonly List<SpriteRenderer> workers = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> locks = new List<SpriteRenderer>();
        SpriteRenderer ghost;
        public void Initialize(GameManager manager)
        {
            this.manager = manager;
            for (int y = 0; y < GridManager.WorldSize; y++) for (int x = 0; x < GridManager.WorldSize; x++)
            {
                var tile = Make("Tile", transform); tile.transform.position = new Vector3(x, y, 0); tile.sortingOrder = -100; tiles.Add(tile);
            }
            ghost = Make("PlacementPreview", transform); ghost.sortingOrder = 100;
            for (int y = 3; y < 21; y += 4) for (int x = 3; x < 21; x += 4)
            { var padlock = Make("Lock", transform); Fit(padlock, manager.Art.Get("icon_lock"), .45f); padlock.transform.position = new Vector3(x, y, 0); padlock.sortingOrder = -90; locks.Add(padlock); }
            manager.Game.Bus.Changed += Refresh; Refresh();
        }
        SpriteRenderer Make(string name, Transform parent)
        {
            var prefab = name == "Crop" || name == "Animal" || name == "Building" || name == "Worker" ? Resources.Load<GameObject>("Prefabs/" + name) : null;
            var instance = prefab != null ? Instantiate(prefab) : new GameObject(name, typeof(SpriteRenderer));
            var item = instance.GetComponent<SpriteRenderer>(); item.transform.SetParent(parent);
            item.sharedMaterial = Resources.Load<Material>("FarmSprite"); return item;
        }
        void Fit(SpriteRenderer view, Sprite sprite, float width)
        {
            view.sprite = sprite; if (sprite != null) view.transform.localScale = Vector3.one * width / sprite.bounds.size.x;
        }
        void Refresh()
        {
            if (manager == null) return;
            var game = manager.Game;
            foreach (var padlock in locks) padlock.gameObject.SetActive(!game.Grid.Unlocked(Mathf.RoundToInt(padlock.transform.position.x), Mathf.RoundToInt(padlock.transform.position.y)));
            for (int y = 0; y < GridManager.WorldSize; y++) for (int x = 0; x < GridManager.WorldSize; x++)
            {
                var view = tiles[y * GridManager.WorldSize + x];
                bool unlocked = game.Grid.Unlocked(x, y);
                Fit(view, manager.Art.Get(game.Grid.Water(x, y) ? "tile_water_01" : "tile_grass_01"), .98f);
                view.color = unlocked ? Color.white : new Color(.42f, .48f, .46f, .65f);
            }
            while (crops.Count < game.State.crops.Count) crops.Add(Make("Crop", transform));
            for (int i = 0; i < crops.Count; i++)
            {
                var view = crops[i]; view.gameObject.SetActive(i < game.State.crops.Count); if (!view.gameObject.activeSelf) continue;
                var crop = game.State.crops[i]; var stage = crop.Stage(game.Data.Crop(crop.definition));
                Fit(view, manager.Art.Get("crop_" + crop.definition + "_" + stage.ToString().ToLowerInvariant() + "_01"), .9f);
                view.transform.position = new Vector3(crop.x, crop.y, 0); view.sortingOrder = 50 - crop.y;
            }
            while (buildings.Count < game.State.buildings.Count) buildings.Add(Make("Building", transform));
            for (int i = 0; i < buildings.Count; i++)
            {
                var view = buildings[i]; view.gameObject.SetActive(i < game.State.buildings.Count); if (!view.gameObject.activeSelf) continue;
                var b = game.State.buildings[i]; var size = PlacementValidator.Size(game.Data.Building(b.definition), b.rotation);
                bool night = TimeManager.Hour(game.State.elapsed) < 6 || TimeManager.Hour(game.State.elapsed) >= 18;
                Fit(view, manager.Art.Get("building_" + b.definition + (night ? "_night" : "_day")), size.x);
                view.transform.position = new Vector3(b.x + (size.x - 1) / 2f, b.y + (size.y - 1) / 2f + .6f, 0); view.sortingOrder = 40 - b.y;
                view.transform.localRotation = Quaternion.Euler(0, 0, b.rotation * 90);
            }
            while (animals.Count < game.State.animals.Count)
            {
                var view = Make("Animal", transform); var behavior = view.GetComponent<AnimalBehavior>() ?? view.gameObject.AddComponent<AnimalBehavior>(); behavior.Bind(manager, game.State.animals[animals.Count]); animals.Add(behavior);
            }
            while (workers.Count < game.State.workers.Count) workers.Add(Make("Worker", transform));
            for (int i = 0; i < workers.Count; i++)
            {
                var w = game.State.workers[i]; var view = workers[i]; Fit(view, manager.Art.Get("player_idle_down"), .55f);
                view.transform.position = new Vector3(w.x + i * .35f + .3f, w.y + .2f, 0); view.sortingOrder = 65 - w.y;
            }
        }
        void Update()
        {
            ghost.gameObject.SetActive(manager.Placement != null);
            if (manager.Placement != null)
            {
                var data = manager.Game.Data.Building(manager.Placement); var size = PlacementValidator.Size(data, manager.Rotation);
                Fit(ghost, manager.Art.Get("building_" + data.id + "_day"), size.x);
                var tile = manager.PlacementTile; ghost.transform.position = new Vector3(tile.x + (size.x - 1) / 2f, tile.y + (size.y - 1) / 2f, 0);
                ghost.transform.rotation = Quaternion.Euler(0, 0, manager.Rotation * 90);
                ghost.color = PlacementValidator.CanPlace(manager.Game.Grid, manager.Game.Data, data, tile.x, tile.y, manager.Rotation) ? new Color(.4f, 1, .4f, .7f) : new Color(1, .3f, .3f, .7f);
            }
            for (int i = 0; i < crops.Count; i++) if (crops[i].gameObject.activeSelf)
            {
                var crop = manager.Game.State.crops[i]; bool ready = crop.Ready(manager.Game.Data.Crop(crop.definition));
                crops[i].transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 2 + i) * (ready ? 4 : 1.5f));
                crops[i].transform.position = new Vector3(crop.x, crop.y + (ready ? Mathf.Abs(Mathf.Sin(Time.time * 3)) * .08f : 0), 0);
            }
        }
        void OnDestroy() { if (manager != null) manager.Game.Bus.Changed -= Refresh; }
    }
}
