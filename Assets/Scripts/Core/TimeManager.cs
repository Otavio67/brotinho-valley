using System;
namespace Brotinho
{
    public static class TimeManager
    {
        public const int DaySeconds = 720, HourSeconds = 30;
        public static int Day(double seconds) => (int)Math.Floor(seconds / DaySeconds);
        public static int Hour(double seconds) => (int)(seconds % DaySeconds / HourSeconds);
        public static double OfflineSeconds(long saved, long now, double cap) => saved <= 0 ? 0 : Math.Max(0, Math.Min(cap, now - saved));
    }
}
