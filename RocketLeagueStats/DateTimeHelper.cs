using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueStats
{
    public static class DateTimeHelper
    {
        public static string ToRfc3339String(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd'T'HH:mm:sszzz", System.Globalization.DateTimeFormatInfo.InvariantInfo);
        }
    }
}
