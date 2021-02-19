using System;

namespace RLStats_Classes.MainClasses
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
        public string SteamId { get; set; }
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
            return Type switch
            {
                "gc" => 16,
                "champion" => 8,
                "diamond" => 4,
                "gold" => 2,
                "regular" => 2,
                _ => throw new Exception("Unknown type: " + Type)
            };
        }
        /// <summary>
        /// returns hour limit measured in calls per hour
        /// </summary>
        /// <returns></returns>
        public int GetHourLimit()
        {
            if (Type is null)
                return 1;
            return Type switch
            {
                "gc" => 0,
                "champion" => 0,
                "diamond" => 2000,
                "gold" => 1000,
                "regular" => 500,
                _ => 1
            };
        }
    }

}