using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Brotinho
{
    public sealed class DayNightCycle : MonoBehaviour
    {
        Light2D light2D;
        void Awake() { light2D = GetComponent<Light2D>(); }
        void Update()
        {
            if (GameManager.Instance == null || light2D == null) return;
            float phase = (float)(GameManager.Instance.Game.State.elapsed % 720 / 720);
            float daylight = Mathf.SmoothStep(0, 1, (Mathf.Sin((phase - .25f) * Mathf.PI * 2) + 1) * .5f);
            light2D.intensity = Mathf.Lerp(light2D.intensity, Mathf.Lerp(.55f, 1, daylight), Time.unscaledDeltaTime);
            light2D.color = Color.Lerp(light2D.color, Color.Lerp(new Color(.65f, .72f, 1), new Color(1, .98f, .88f), daylight), Time.unscaledDeltaTime);
        }
    }
}
