using System.Collections.Generic;
using UnityEngine;
namespace Brotinho
{
    public sealed class GridManager
    {
        public const int WorldSize = 21;
        readonly GameState state;
        public GridManager(GameState state) { this.state = state; }
        public bool Inside(int x, int y) => x >= 0 && y >= 0 && x < WorldSize && y < WorldSize;
        public bool Unlocked(int x, int y)
        {
            if (!Inside(x, y)) return false;
            if (x >= 8 && x <= 12 && y >= 8 && y <= 12) return true;
            foreach (var block in state.land) if (x >= block.x && x < block.x + 4 && y >= block.y && y < block.y + 4) return true;
            return false;
        }
        public bool Water(int x, int y) => state.region == "riverside" && x == 7 || state.region != "riverside" && x == 1 && y % 3 != 0;
        public CropInstance Crop(int x, int y) { foreach (var crop in state.crops) if (crop.x == x && crop.y == y) return crop; return null; }
        public BuildingInstance Building(int x, int y, GameDatabase data)
        {
            foreach (var b in state.buildings)
            {
                var d = data.Building(b.definition); var size = PlacementValidator.Size(d, b.rotation);
                if (x >= b.x && x < b.x + size.x && y >= b.y && y < b.y + size.y) return b;
            }
            return null;
        }
        public TileState State(int x, int y, GameDatabase data)
        {
            if (!Unlocked(x, y)) return TileState.Locked;
            if (Water(x, y)) return TileState.Water;
            if (Building(x, y, data) != null) return TileState.Building;
            return Crop(x, y) != null ? TileState.Crop : TileState.Empty;
        }
        public List<Vector2Int> Path(Vector2Int start, Vector2Int destination, GameDatabase data)
        {
            var frontier = new Queue<Vector2Int>(); var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            frontier.Enqueue(start); cameFrom[start] = start;
            var dirs = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue(); if (current == destination) break;
                foreach (var dir in dirs)
                {
                    var next = current + dir; var tile = State(next.x, next.y, data);
                    if (tile == TileState.Locked || tile == TileState.Water || tile == TileState.Building || cameFrom.ContainsKey(next)) continue;
                    cameFrom[next] = current; frontier.Enqueue(next);
                }
            }
            var result = new List<Vector2Int>(); if (!cameFrom.ContainsKey(destination)) return result;
            for (var p = destination; p != start; p = cameFrom[p]) result.Add(p);
            result.Reverse(); return result;
        }
    }
}
