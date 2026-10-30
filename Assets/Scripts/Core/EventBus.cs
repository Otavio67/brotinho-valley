using System;

namespace Brotinho
{
    // Instance-owned bus: subscriptions cannot survive a reset or a new game session.
    public sealed class EventBus
    {
        public event Action Changed;
        public event Action<string> Toast;
        public event Action<string> OpenPanel;
        public event Action<SfxEvent> Sound;
        public void Refresh() => Changed?.Invoke();
        public void Notify(string key) => Toast?.Invoke(key);
        public void Open(string panel) => OpenPanel?.Invoke(panel);
        public void Play(SfxEvent sound) => Sound?.Invoke(sound);
    }
    public enum SfxEvent { Plant, Water, Harvest, Sell, Buy, Build, LevelUp, Achievement, Error, Click, Notification, Chicken, Cow, Rabbit }
    public interface IRewardedAdProvider { }
    public interface IPurchaseProvider { }
}
