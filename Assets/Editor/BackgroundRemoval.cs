using System.Collections.Generic;
using System.IO;
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
                    int i = queue.Dequeue(); pixels[i].a = 0;
                    if (i % width > 0) Enqueue(i - 1); if (i % width < width - 1) Enqueue(i + 1);
                    Enqueue(i - width); Enqueue(i + width);
                }
                // Loading a JPEG converts Texture2D to RGB24, so alpha must be written to a separate RGBA texture.
                var output = new Texture2D(width, height, TextureFormat.RGBA32, false);
                output.SetPixels32(pixels); output.Apply(); File.WriteAllBytes("Assets/Art/Transparent/" + name + ".png", output.EncodeToPNG());
                Object.DestroyImmediate(output); Object.DestroyImmediate(texture);
            }
            AssetDatabase.Refresh();
        }
    }
}
