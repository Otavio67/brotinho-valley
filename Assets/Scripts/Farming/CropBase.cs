using UnityEngine;
namespace Brotinho
{
    [CreateAssetMenu(menuName = "Brotinho/Crop")]
    public sealed class CropBase : ScriptableObject
    {
        public string id, product;
        public int seedCost, yield;
        public double growthSeconds, regrowSeconds;
        public int unlockLevel = 1;
    }
}
