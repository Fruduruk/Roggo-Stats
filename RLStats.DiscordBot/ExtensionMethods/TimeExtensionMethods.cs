using Discord_Bot.RLStats;

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
        /// <summary>
        /// Converts d,w,m or y to timeRange.
        /// </summary>
        /// <param name="time">d,w,m or y</param>
        /// <returns>Returns this {day, week, month ...} so Now - one day or one month. The range of that </returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if time is not d,w,m or y</exception>
        public static TimeRange ConvertToThisTimeRange(this string time)
        {
            TimeRange startTimeRange;
            switch (time)
            {
                case "d":
                case "day":
                    startTimeRange = TimeRange.Today;
                    break;
                case "w":
                case "week":
                    startTimeRange = TimeRange.ThisWeek;
                    break;
                case "m":
                case "month":
                    startTimeRange = TimeRange.ThisMonth;
                    break;
                case "y":
                case "year":
                    startTimeRange = TimeRange.ThisYear;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{time} is not a valid time parameter. Use d,w,m or y.");
            }
            return startTimeRange;
        }

        /// <summary>
        /// Converts d,w,m or y to timeRange.
        /// </summary>
        /// <param name="time">d,w,m or y</param>
        /// <returns>Returns last {day, week, month ...} so Now - one day or one month... - one day or one month... The range of that </returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if time is not d,w,m or y</exception>
        public static TimeRange ConvertToLastTimeRange(this string time)
        {
            TimeRange startTimeRange;
            switch (time)
            {
                case "d":
                case "day":
                    startTimeRange = TimeRange.Yesterday;
                    break;
                case "w":
                case "week":
                    startTimeRange = TimeRange.LastWeek;
                    break;
                case "m":
                case "month":
                    startTimeRange = TimeRange.LastMonth;
                    break;
                case "y":
                case "year":
                    startTimeRange = TimeRange.LastYear;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{time} is not a valid time parameter. Use d,w,m or y.");
            }
            return startTimeRange;
        }

        public static bool CanConvertTime(this string time)
        {
            try
            {
                _ = ConvertToThisTimeRange(time);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public static Tuple<DateTime, DateTime> ConvertToDateTimeRange(this TimeRange timeRange)
        {
            switch (timeRange)
            {
                case TimeRange.Today:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today,
                        DateTime.Today + new TimeSpan(1, 0, 0, 0)
                        );
                case TimeRange.Yesterday:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(1, 0, 0, 0)),
                        DateTime.Today
                        );
                case TimeRange.ThisWeek:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(7, 0, 0, 0)),
                        DateTime.Today
                    );
                case TimeRange.LastWeek:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(15, 0, 0, 0)),
                        DateTime.Today.Subtract(new TimeSpan(8, 0, 0, 0))
                    );
                case TimeRange.ThisMonth:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(28, 0, 0, 0)),
                        DateTime.Today
                    );
                case TimeRange.LastMonth:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(57, 0, 0, 0)),
                        DateTime.Today.Subtract(new TimeSpan(29, 0, 0, 0))
                    );
                case TimeRange.ThisYear:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(365, 0, 0, 0)),
                        DateTime.Today
                    );
                case TimeRange.LastYear:
                    return new Tuple<DateTime, DateTime>(
                        DateTime.Today.Subtract(new TimeSpan(731, 0, 0, 0)),
                        DateTime.Today.Subtract(new TimeSpan(366, 0, 0, 0))
                    );
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeRange), timeRange, null);
            }
        }
    }
}
