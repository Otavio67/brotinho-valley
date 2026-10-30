using UnityEngine;
namespace Brotinho
{
    public sealed class AnimalBehavior : MonoBehaviour
    {
        GameManager manager;
        AnimalInstance animal;
        BuildingInstance home;
        SpriteRenderer view;
        string lastState;
        double nextFrame;
        public void Bind(GameManager manager, AnimalInstance animal)
        {
            this.manager = manager; this.animal = animal; home = manager.Game.State.buildings.Find(x => x.id == animal.building);
            view = GetComponent<SpriteRenderer>(); lastState = null;
        }
        void Update()
        {
            if (home == null || manager == null) return;
            double t = manager.Game.State.elapsed; double phase = (t + home.x * 11 + home.y) % 180;
            string state = phase > 173 ? "funny" : phase > 160 ? "sleep" : animal.feed == 0 ? "eat" : phase % 24 < 5 ? "rest" : phase % 24 < 12 ? "idle" : "walk";
            string direction = Mathf.Sin((float)t * .25f) >= 0 ? "right" : "left";
            if (Time.unscaledTimeAsDouble >= nextFrame || state != lastState)
            {
                nextFrame = Time.unscaledTimeAsDouble + .18;
                string key = "animal_" + animal.definition + "_" + state + "_" + direction;
                if (state == "walk") key += "_" + ((int)(t * 5) % 4 + 1).ToString("00");
                view.sprite = manager.Art.Get(key); lastState = state;
                if (view.sprite != null) transform.localScale = Vector3.one * .65f / view.sprite.bounds.size.x;
            }
            float wander = state == "walk" ? Mathf.Sin((float)t * .7f + home.x) * .3f : 0;
            transform.position = new Vector3(home.x + .5f + wander, home.y + .3f + Mathf.Cos((float)t * .3f) * .2f, 0);
            view.sortingOrder = 60 - home.y;
        }
    }
}
