using UnityEngine;
namespace Brotinho
{
    [RequireComponent(typeof(UnityEngine.UI.GridLayoutGroup))]
    public sealed class ResponsiveGrid : MonoBehaviour
    {
        RectTransform rect;
        UnityEngine.UI.GridLayoutGroup grid;
        float width;
        void Awake() { rect = (RectTransform)transform; grid = GetComponent<UnityEngine.UI.GridLayoutGroup>(); }
        void LateUpdate()
        {
            if (Mathf.Approximately(width, rect.rect.width)) return; width = rect.rect.width;
            grid.cellSize = new Vector2(Mathf.Max(80, (width - grid.spacing.x) / 2), grid.cellSize.y);
        }
    }
}
