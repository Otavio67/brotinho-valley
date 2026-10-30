using UnityEngine;
namespace Brotinho
{
    [CreateAssetMenu(menuName = "Brotinho/Product")]
    public sealed class ProductData : ScriptableObject { public string id; public int basePrice; }
}
