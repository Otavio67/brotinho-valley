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
        public bool Silent;
        public void Refresh() { if (!Silent) Changed?.Invoke(); }
        public void Notify(string key) { if (!Silent) Toast?.Invoke(key); }
        public void Open(string panel) { if (!Silent) OpenPanel?.Invoke(panel); }
        public void Play(SfxEvent sound) { if (!Silent) Sound?.Invoke(sound); }
    }
    public enum SfxEvent { Plant, Water, Harvest, Sell, Buy, Build, LevelUp, Achievement, Error, Click, Notification, Chicken, Cow, Rabbit }
    public interface IRewardedAdProvider { }
    public interface IPurchaseProvider { }
}
