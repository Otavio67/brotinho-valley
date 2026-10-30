using UnityEngine;
namespace Brotinho
{
    [CreateAssetMenu(menuName = "Brotinho/Achievement")]
    public sealed class AchievementData : ScriptableObject { public string id, metric; public int target = 1, gems = 1; }
}
