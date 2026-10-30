using UnityEngine;
namespace Brotinho
{
    [CreateAssetMenu(menuName = "Brotinho/Villager")]
    public sealed class VillagerData : ScriptableObject { public string id; public Sprite portrait; public string greetingKey; }
}
