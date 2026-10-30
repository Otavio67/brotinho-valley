using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Brotinho.Editor
{
    public static class BackgroundRemoval
    {
        // User requested pixel-preserving background removal, never a redraw. Sources stay byte-for-byte intact.
        // Only green pixels connected to a sheet edge are cleared; outlined green leaves remain enclosed.
        [MenuItem("Brotinho/Remove sprite backgrounds")]
        public static void Run()
        {
            Directory.CreateDirectory("Assets/Art/Transparent");
            var names = new[] { "wheat", "lettuce", "carrot", "apple_tree", "chicken", "cow", "rabbit", "pig", "my_farmer", "barn", "chicken-coop", "farmhouse", "silo", "water-well", "windmill" };
            foreach (string name in names)
            {
                string path = "Assets/Art/Source/" + name + ".jpeg";
                if (!File.Exists(path)) continue;
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false); texture.LoadImage(File.ReadAllBytes(path));
                var pixels = texture.GetPixels32(); var visited = new bool[pixels.Length]; var queue = new Queue<int>();
                int width = texture.width, height = texture.height;
                void Enqueue(int index)
                {
                    if (index < 0 || index >= pixels.Length || visited[index]) return;
                    visited[index] = true; Color32 c = pixels[index];
                    if (c.g > 105 && c.g > c.r * 1.45f && c.g > c.b * 1.45f) queue.Enqueue(index);
                }
                for (int x = 0; x < width; x++) { Enqueue(x); Enqueue((height - 1) * width + x); }
                for (int y = 0; y < height; y++) { Enqueue(y * width); Enqueue(y * width + width - 1); }
                while (queue.Count > 0)
                {
                    int i = queue.Dequeue(); pixels[i] = new Color32(0, 0, 0, 0);
                    if (i % width > 0) Enqueue(i - 1); if (i % width < width - 1) Enqueue(i + 1);
                    Enqueue(i - width); Enqueue(i + width);
                }
                // Loading a JPEG converts Texture2D to RGB24, so alpha must be written to a separate RGBA texture.
                var output = new Texture2D(width, height, TextureFormat.RGBA32, false);
                output.SetPixels32(pixels); output.Apply(); WriteIfChanged("Assets/Art/Transparent/" + name + ".png", output.EncodeToPNG());
                Object.DestroyImmediate(output); Object.DestroyImmediate(texture);
            }
            AssetDatabase.Refresh();
        }
        public static string[] CutoutUI(string source, string[] names, Rect[] rectangles)
        {
            const string folder = "Assets/Art/Transparent/UI/"; Directory.CreateDirectory(folder);
            var sheet = new Texture2D(2, 2); sheet.LoadImage(File.ReadAllBytes(source)); var sourcePixels = sheet.GetPixels32();
            var paths = new string[names.Length];
            for (int n = 0; n < names.Length; n++)
            {
                var rect = rectangles[n]; int width = (int)rect.width, height = (int)rect.height, left = (int)rect.x, bottom = (int)rect.y;
                var pixels = new Color32[width * height]; var visited = new bool[pixels.Length]; var queue = new Queue<int>();
                for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) pixels[y * width + x] = sourcePixels[(bottom + y) * sheet.width + left + x];
                void Enqueue(int index)
                {
                    if (index < 0 || index >= pixels.Length || visited[index]) return; visited[index] = true;
                    var c = pixels[index]; if (c.r > 205 && c.g > 180 && c.b > 130 && c.r - c.b < 125) queue.Enqueue(index);
                }
                for (int x = 0; x < width; x++) { Enqueue(x); Enqueue((height - 1) * width + x); }
                for (int y = 0; y < height; y++) { Enqueue(y * width); Enqueue(y * width + width - 1); }
                while (queue.Count > 0) { int i = queue.Dequeue(); pixels[i] = new Color32(0, 0, 0, 0); if (i % width > 0) Enqueue(i - 1); if (i % width < width - 1) Enqueue(i + 1); Enqueue(i - width); Enqueue(i + width); }
                var output = new Texture2D(width, height, TextureFormat.RGBA32, false); output.SetPixels32(pixels); output.Apply();
                paths[n] = folder + names[n] + ".png"; WriteIfChanged(paths[n], output.EncodeToPNG()); Object.DestroyImmediate(output);
            }
            Object.DestroyImmediate(sheet); AssetDatabase.Refresh(); return paths;
        }
        public static void WriteIfChanged(string path, byte[] bytes)
        {
            if (File.Exists(path) && File.ReadAllBytes(path).SequenceEqual(bytes)) return;
            AssetDatabase.ReleaseCachedFileHandles(); string temp = path + ".tmp"; File.WriteAllBytes(temp, bytes);
            if (File.Exists(path)) File.Replace(temp, path, null); else File.Move(temp, path);
        }
    }
}
