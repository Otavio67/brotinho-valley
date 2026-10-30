using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace Brotinho
{
    public sealed class PlayerController : MonoBehaviour
    {
        GameManager manager;
        Vector2 start, previous;
        double started;
        bool pressed, dragged, blocked;
        Camera view;
        public void Initialize(GameManager manager) { this.manager = manager; view = GetComponent<Camera>(); }
        void Update()
        {
            var pointer = Pointer.current; if (pointer == null) return;
            Vector2 position = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame)
            {
                start = previous = position; started = Time.unscaledTimeAsDouble;
                pressed = true; dragged = false;
                blocked = EventSystem.current != null && (EventSystem.current.IsPointerOverGameObject() || GameUI.ModalOpen);
            }
            if (!pressed || blocked) return;
            if (pointer.press.isPressed && Vector2.Distance(start, position) > 12) dragged = true;
            if (dragged && pointer.press.isPressed)
            {
                if (manager.Placement != null) manager.PlacementTile = Tile(position);
                else
                {
                    Vector3 delta = view.ScreenToWorldPoint(previous) - view.ScreenToWorldPoint(position);
                    var p = view.transform.position + delta;
                    p.x = Mathf.Clamp(p.x, 0, 20); p.y = Mathf.Clamp(p.y, 0, 20); p.z = -10; view.transform.position = p;
                }
            }
            previous = position;
            if (!pointer.press.wasReleasedThisFrame) return;
            pressed = false; if (dragged) return;
            var tile = Tile(position);
            if (Time.unscaledTimeAsDouble - started >= .55)
            {
                var crop = manager.Game.Grid.Crop(tile.x, tile.y);
                manager.Game.Bus.Notify(crop == null ? "empty_tile" : "crop_" + crop.definition);
                return;
            }
            if (manager.Placement != null) { manager.PlacementTile = tile; return; }
            PlayerTools.Apply(manager, tile.x, tile.y);
        }
        Vector2Int Tile(Vector2 screen) { var world = view.ScreenToWorldPoint(screen); return new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y)); }
    }
}
