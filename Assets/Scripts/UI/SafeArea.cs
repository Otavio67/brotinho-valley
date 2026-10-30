using UnityEngine;
namespace Brotinho
{
    public sealed class SafeArea : MonoBehaviour
    {
        Rect last; Vector2 size;
        void Update()
        {
            if (last == Screen.safeArea && size.x == Screen.width && size.y == Screen.height) return;
            last = Screen.safeArea; size = new Vector2(Screen.width, Screen.height);
            if (size.x <= 0 || size.y <= 0) return;
            var rect = (RectTransform)transform; rect.anchorMin = last.min / size; rect.anchorMax = last.max / size; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
