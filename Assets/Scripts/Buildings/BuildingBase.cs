using UnityEngine;
namespace Brotinho
{
    [CreateAssetMenu(menuName = "Brotinho/Building")]
    public sealed class BuildingBase : ScriptableObject
    {
        public string id, output;
        public int cost, unlockLevel = 1, outputCount = 1;
        public Vector2Int size = new Vector2Int(2, 2);
        public double processSeconds;
        public ItemStack[] inputs = new ItemStack[0];
        public bool animalHousing, decoration;
        public string region;
    }
}
