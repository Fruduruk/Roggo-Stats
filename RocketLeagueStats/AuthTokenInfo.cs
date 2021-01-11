using System;

namespace RocketLeagueStats
{
    public class AuthTokenInfo
    {
        public AuthTokenInfo(string token)
        {
            Token = token;
        }

        public string Token { get; }
        public Exception Except { get; set; }
        public bool Chaser { get; set; }
        public string SteamID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        /// <summary>
        /// Returns speed measured in calls per second
        /// </summary>
        /// <returns></returns>
        public int GetSpeed()
        {
            if (Type is null)
                throw new Exception("Type was null");
            switch (Type)
            {
                case "gc":
                    return 16;
                case "champion":
                    return 8;
                case "diamond":
                    return 4;
                case "gold":
                    return 2;
                case "regular":
                    return 2;
                default:
                    throw new Exception("Unknown type: "+Type);
            }
        }
        /// <summary>
        /// returns hour limit measured in calls per hour
        /// </summary>
        /// <returns></returns>
        public int GetHourLimit()
        {
            if (Type is null)
                return 1;
            switch (Type)
            {
                case "gc":
                    return 0;
                case "champion":
                    return 0;
                case "diamond":
                    return 2000;
                case "gold":
                    return 1000;
                case "regular":
                    return 500;
                default:
                    return 1;
            }
        }
    }

}