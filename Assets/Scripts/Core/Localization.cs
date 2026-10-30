using System.Collections.Generic;
using UnityEngine;
namespace Brotinho
{
    public sealed class Localization
    {
        readonly Dictionary<string, string[]> entries = new Dictionary<string, string[]>();
        readonly GameState state;
        public Localization(GameState state)
        {
            this.state = state;
            var file = Resources.Load<TextAsset>("Localization");
            if (file == null) return;
            foreach (var line in file.text.Split('\n'))
            {
                var cells = line.TrimEnd('\r').Split('\t');
                if (cells.Length >= 3 && !cells[0].StartsWith("#")) entries[cells[0]] = cells;
            }
        }
        public string Get(string key) => entries.TryGetValue(key, out var value) ? value[state.language == "en" ? 2 : 1] : key;
        public string Format(string key, params object[] values) => string.Format(Get(key), values);
    }
}
