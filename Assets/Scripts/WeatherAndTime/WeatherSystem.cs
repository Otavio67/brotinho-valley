namespace Brotinho
{
    public enum WeatherKind { Sunny, Cloudy, Rain, Storm, Dew }
    public static class WeatherSystem
    {
        public static WeatherKind At(double time)
        {
            if (time < 30) return WeatherKind.Sunny;
            uint hour = (uint)(time / TimeManager.HourSeconds);
            uint hash = hour * 747796405u + 2891336453u;
            hash = ((hash >> (int)((hash >> 28) + 4)) ^ hash) * 277803737u;
            int roll = (int)(((hash >> 22) ^ hash) % 100);
            if (TimeManager.Hour(time) < 3 && roll < 40) return WeatherKind.Dew;
            return roll < 55 ? WeatherKind.Sunny : roll < 78 ? WeatherKind.Cloudy : roll < 97 ? WeatherKind.Rain : WeatherKind.Storm;
        }
        // Storms are rare; a severe hour and the following hour form a 60-second harvest lock.
        public static bool HarvestBlocked(double time) => At(time) == WeatherKind.Storm || time >= 30 && At(time - 30) == WeatherKind.Storm;
    }
}
