using System;

namespace Discord_Bot.ExtensionMethods
{
    public static class TimeExtensionMethods
    {
        public static TimeSpan ConvertTimeToTimeSpan(this string time)
        {
            return time switch
            {
                "d" => TimeSpan.FromDays(1),
                "w" => TimeSpan.FromDays(7),
                "m" => TimeSpan.FromDays(28),
                "y" => TimeSpan.FromDays(365),
                _ => TimeSpan.FromDays(int.MaxValue),
            };
        }

        public static string Adverbify(this string time)
        {
            return time switch
            {
                "d" => "daily",
                "w" => "weekly",
                "m" => "monthly",
                "y" => "yearly",
                _ => "dunno man",
            };
        }
    }
}
