using System;
using System.Collections.Generic;
namespace Brotinho
{
    [Serializable] public sealed class WorkerInstance
    {
        public string definition;
        public WorkerTask task;
        public string crop = "lettuce";
        public int x = 8, y = 8, width = 5, height = 5;
        public double energy = 100, cooldown;
        public bool resting;
        public List<string> history = new List<string>();
    }
}
