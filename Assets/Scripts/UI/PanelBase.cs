using System.Collections;
using UnityEngine;
namespace Brotinho
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelBase : MonoBehaviour
    {
        public void Open() { gameObject.SetActive(true); StopAllCoroutines(); StartCoroutine(Animate(true)); }
        public void Close() { StopAllCoroutines(); StartCoroutine(Animate(false)); }
        IEnumerator Animate(bool opening)
        {
            var group = GetComponent<CanvasGroup>(); group.blocksRaycasts = opening; group.interactable = opening;
            float t = 0; while (t < .15f)
            {
                t += Time.unscaledDeltaTime; float a = Mathf.Clamp01(t / .15f);
                group.alpha = opening ? a : 1 - a;
                transform.localScale = Vector3.one * Mathf.Lerp(opening ? .97f : 1, opening ? 1 : .97f, a); yield return null;
            }
            if (!opening) gameObject.SetActive(false);
        }
    }
}
