using UnityEngine;
namespace Brotinho
{
    [CreateAssetMenu(menuName = "Brotinho/Region")]
    public sealed class RegionData : ScriptableObject { public string id; public Color color = Color.green; }
}
