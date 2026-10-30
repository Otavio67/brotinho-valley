using UnityEngine;
namespace Brotinho
{
    [CreateAssetMenu(menuName = "Brotinho/Animal")]
    public sealed class AnimalBase : ScriptableObject { public string id, housing, product, feed; public int cost; public double interval; }
}
