using System;

namespace RLStats_Classes.MainClasses
{
    public static class DateTimeHelper
    {
        public static string ToRfc3339String(this DateTime dateTime)
        {
            var time = dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            time += "-00:00";
            return time;
        }
    }
}
