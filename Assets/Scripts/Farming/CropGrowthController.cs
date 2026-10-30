using System;
namespace Brotinho
{
    public static class CropGrowthController
    {
        public static double RegionSpeed(string region, string crop)
        {
            if (region == "riverside" && (crop == "wheat" || crop == "lettuce")) return 1.15;
            if (region == "forest" && crop == "carrot") return .85;
            return 1;
        }
        public static void Advance(CropInstance crop, CropBase data, string region, double start, double seconds, bool rain)
        {
            // Integrate the watered part separately so a bonus cannot outlive its 60-second window.
            double wetSeconds = Math.Max(0, Math.Min(seconds, crop.wateredUntil - start));
            crop.growth = Math.Min(crop.Duration(data), crop.growth + (seconds + wetSeconds * .25) * RegionSpeed(region, data.id) * (rain ? 1.2 : 1));
        }
    }
}
