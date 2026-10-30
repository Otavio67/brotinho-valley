using System.Collections.Generic;
using UnityEngine;
namespace Brotinho
{
    public sealed class PlayerMovement : MonoBehaviour
    {
        GameManager manager;
        SpriteRenderer view;
        List<Vector2Int> path = new List<Vector2Int>();
        int next;
        string facing = "down";
        float frameAt;
        public void Initialize(GameManager manager)
        {
            this.manager = manager; view = GetComponent<SpriteRenderer>(); view.sharedMaterial = Resources.Load<Material>("FarmSprite");
            transform.position = new Vector3(manager.Game.State.playerX, manager.Game.State.playerY, 0); SetSprite("idle");
        }
        public void MoveTo(int x, int y)
        {
            var start = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
            path = manager.Game.Grid.Path(start, new Vector2Int(x, y), manager.Game.Data); next = 0;
        }
        void SetSprite(string state)
        {
            string key = "player_" + state + "_" + facing;
            if (state == "walk") key += "_" + ((int)(Time.unscaledTime * 6) % 4 + 1).ToString("00");
            view.sprite = manager.Art.Get(key); if (view.sprite != null) transform.localScale = Vector3.one * .65f / view.sprite.bounds.size.x;
        }
        void Update()
        {
            if (manager == null) return;
            if (next < path.Count)
            {
                var target = (Vector3)(Vector2)path[next];
                // Revalidate paths because construction can happen after a route was planned.
                var tile = manager.Game.Grid.State(path[next].x, path[next].y, manager.Game.Data);
                if (tile == TileState.Building || tile == TileState.Water || tile == TileState.Locked) { path.Clear(); next = 0; return; }
                var delta = target - transform.position; facing = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? delta.x > 0 ? "right" : "left" : delta.y > 0 ? "up" : "down";
                transform.position = Vector3.MoveTowards(transform.position, target, Time.unscaledDeltaTime * 3);
                if ((transform.position - target).sqrMagnitude < .001f) { manager.Game.State.playerX = path[next].x; manager.Game.State.playerY = path[next].y; next++; }
            }
            if (Time.unscaledTime > frameAt) { frameAt = Time.unscaledTime + .16f; SetSprite(next < path.Count ? "walk" : "idle"); }
            view.sortingOrder = 75 - Mathf.RoundToInt(transform.position.y);
        }
    }
}
