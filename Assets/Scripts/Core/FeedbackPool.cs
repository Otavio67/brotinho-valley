using UnityEngine;
namespace Brotinho
{
    // Twelve recycled sprite particles provide action feedback without Instantiate/Destroy during play.
    public sealed class FeedbackPool : MonoBehaviour
    {
        GameManager manager;
        readonly SpriteRenderer[] particles = new SpriteRenderer[12];
        readonly float[] lifetime = new float[12];
        int next;
        public void Initialize(GameManager manager)
        {
            this.manager = manager;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = new GameObject("FeedbackParticle").AddComponent<SpriteRenderer>(); particles[i].transform.SetParent(transform);
                particles[i].sortingOrder = 200; particles[i].enabled = false;
            }
            manager.Game.Bus.Feedback += Emit;
        }
        void Emit(Vector2 position, SfxEvent sound)
        {
            int index = next++ % particles.Length; var p = particles[index];
            p.sprite = manager.Art.Get(sound == SfxEvent.Water ? "icon_water" : sound == SfxEvent.Harvest ? "icon_harvest" : sound == SfxEvent.Build ? "icon_build" : "icon_plant");
            p.transform.position = position + Vector2.up * .35f; p.transform.localScale = Vector3.one * .4f / p.sprite.bounds.size.x;
            p.color = Color.white; p.enabled = true; lifetime[index] = .7f;
        }
        void Update()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (lifetime[i] <= 0) continue; lifetime[i] -= Time.unscaledDeltaTime;
                if (lifetime[i] <= 0) { particles[i].enabled = false; continue; }
                particles[i].transform.position += Vector3.up * (Time.unscaledDeltaTime * .7f); particles[i].color = new Color(1, 1, 1, lifetime[i] / .7f);
            }
        }
        void OnDestroy() { if (manager != null) manager.Game.Bus.Feedback -= Emit; }
    }
}
