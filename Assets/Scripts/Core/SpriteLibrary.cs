using System;
using System.Collections.Generic;
using UnityEngine;
namespace Brotinho
{
    [CreateAssetMenu(menuName = "Brotinho/Sprite library")]
    public sealed class SpriteLibrary : ScriptableObject
    {
        [Serializable] public struct Entry { public string name; public Sprite sprite; }
        public Sprite fallback;
        public Entry[] entries = Array.Empty<Entry>();
        readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        readonly HashSet<string> warned = new HashSet<string>();
        public Sprite Get(string key)
        {
            if (cache.Count == 0) foreach (var entry in entries) cache[entry.name] = entry.sprite;
            if (cache.TryGetValue(key, out var sprite) && sprite != null) return sprite;
            if (warned.Add(key)) UnityEngine.Debug.LogWarning("Brotinho: missing sprite " + key, this);
            return fallback;
        }
        void OnEnable() { cache.Clear(); warned.Clear(); }
    }
}
