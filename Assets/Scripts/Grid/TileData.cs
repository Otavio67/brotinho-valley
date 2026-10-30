namespace Brotinho
{
    public enum TileState { Locked, Empty, Water, Crop, Building }
    public readonly struct TileData
    {
        public readonly int X, Y;
        public readonly TileState State;
        public TileData(int x, int y, TileState state) { X = x; Y = y; State = state; }
    }
}
