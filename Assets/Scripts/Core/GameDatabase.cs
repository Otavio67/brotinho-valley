using System;
using UnityEngine;
namespace Brotinho
{
    [CreateAssetMenu(menuName = "Brotinho/Database")]
    public sealed class GameDatabase : ScriptableObject
    {
        public CropBase[] crops = Array.Empty<CropBase>();
        public BuildingBase[] buildings = Array.Empty<BuildingBase>();
        public ProductData[] products = Array.Empty<ProductData>();
        public AnimalBase[] animals = Array.Empty<AnimalBase>();
        public WorkerData[] workers = Array.Empty<WorkerData>();
        public RegionData[] regions = Array.Empty<RegionData>();
        public AchievementData[] achievements = Array.Empty<AchievementData>();
        public VillagerData[] villagers = Array.Empty<VillagerData>();
        public double offlineCapSeconds = 8 * 3600;
        public CropBase Crop(string id) => Array.Find(crops, x => x.id == id);
        public BuildingBase Building(string id) => Array.Find(buildings, x => x.id == id);
        public ProductData Product(string id) => Array.Find(products, x => x.id == id);
        public AnimalBase Animal(string id) => Array.Find(animals, x => x.id == id);
        public RegionData Region(string id) => Array.Find(regions, x => x.id == id);
    }
}
