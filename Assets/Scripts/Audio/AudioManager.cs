using UnityEngine;
namespace Brotinho
{
    public sealed class AudioManager : MonoBehaviour
    {
        GameManager manager;
        AudioSource sfx, day, night;
        AudioClip[] clips;
        public void Initialize(GameManager manager)
        {
            this.manager = manager; sfx = gameObject.AddComponent<AudioSource>();
            day = gameObject.AddComponent<AudioSource>(); night = gameObject.AddComponent<AudioSource>();
            day.loop = night.loop = true; day.volume = night.volume = 0;
            day.clip = Resources.Load<AudioClip>("Audio/music_day"); night.clip = Resources.Load<AudioClip>("Audio/music_night");
            if (day.clip != null) day.Play(); if (night.clip != null) night.Play();
            var names = System.Enum.GetNames(typeof(SfxEvent)); clips = new AudioClip[names.Length];
            for (int i = 0; i < names.Length; i++) clips[i] = Resources.Load<AudioClip>("Audio/sfx_" + names[i].ToLowerInvariant());
            manager.Game.Bus.Sound += Play;
        }
        void Play(SfxEvent sound)
        {
            if (manager.Game.State.sound && clips[(int)sound] != null) sfx.PlayOneShot(clips[(int)sound]);
            if (manager.Game.State.vibration && Application.isMobilePlatform && sound == SfxEvent.Harvest) Handheld.Vibrate();
        }
        void Update()
        {
            if (manager == null) return; int hour = TimeManager.Hour(manager.Game.State.elapsed);
            bool daytime = hour >= 6 && hour < 18; bool enabled = manager.Game.State.music;
            day.volume = Mathf.MoveTowards(day.volume, enabled && daytime ? .4f : 0, Time.unscaledDeltaTime * .2f);
            night.volume = Mathf.MoveTowards(night.volume, enabled && !daytime ? .4f : 0, Time.unscaledDeltaTime * .2f);
        }
        void OnDestroy() { if (manager != null) manager.Game.Bus.Sound -= Play; }
    }
}
