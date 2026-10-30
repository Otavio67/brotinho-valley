using UnityEngine;
namespace Brotinho
{
    public static class PlacementValidator
    {
        public static Vector2Int Size(BuildingBase building, int rotation) => rotation % 2 == 0 ? building.size : new Vector2Int(building.size.y, building.size.x);
        public static bool CanPlace(GridManager grid, GameDatabase data, BuildingBase building, int x, int y, int rotation)
        {
            var size = Size(building, rotation);
            for (int i = x; i < x + size.x; i++) for (int j = y; j < y + size.y; j++)
                if (grid.State(i, j, data) != TileState.Empty) return false;
            return true;
        }
    }
}
