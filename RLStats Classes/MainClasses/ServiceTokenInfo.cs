using RLStats_Classes.MainClasses.Interfaces;
using System;

namespace RLStats_Classes.MainClasses
{
    public class ServiceTokenInfo : IAuthTokenInfo
    {
        public string Token { get; }
        public string Type { get; set; }

        public ServiceTokenInfo(string token)
        {
            Token = token;
        }
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
