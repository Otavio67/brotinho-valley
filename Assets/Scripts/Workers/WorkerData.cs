using UnityEngine;
namespace Brotinho
{
    public enum WorkerSpecialty { Farming, Animals, Processing, Selling }
    public enum WorkerTask { Rest, Farm, Animals, Process, Sell }
    [CreateAssetMenu(menuName = "Brotinho/Worker")]
    public sealed class WorkerData : ScriptableObject { public string id, personality; public WorkerSpecialty specialty; public float speed = 1; public Sprite portrait; }
}
