using System;
namespace Brotinho
{
    public enum CropStage { Planted, Sprout, Growing, Grown, Ready }
    [Serializable] public sealed class CropInstance
    {
        public string id, definition;
        public int x, y;
        public double growth, wateredUntil;
        public bool regrowing;
        public double Duration(CropBase data) => regrowing ? data.regrowSeconds : data.growthSeconds;
        public CropStage Stage(CropBase data) => Ready(data) ? CropStage.Ready : (CropStage)Math.Min(3, (int)(growth / Duration(data) * 4));
        public bool Ready(CropBase data) => growth >= Duration(data);
    }
}
