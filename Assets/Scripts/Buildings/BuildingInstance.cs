using System;
namespace Brotinho
{
    [Serializable] public sealed class BuildingInstance
    {
        public string id, definition;
        public int x, y, rotation, queue, output;
        public double progress;
    }
}
