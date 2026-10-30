using System;
namespace Brotinho
{
    [Serializable] public sealed class AnimalInstance
    {
        public string id, definition, building;
        public double progress, bonusRemainder;
        public int feed = 3, ready;
    }
}
