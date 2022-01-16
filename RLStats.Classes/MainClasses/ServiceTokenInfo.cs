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
        public double GetSpeed()
        {
            return 0.034d;
        }
        public double GetHourLimit()
        {
            if (Type is null)
                throw new Exception("Type was null");
            return Type switch
            {
                "gc" => 0,
                "champion" => 0,
                "diamond" => 2000,
                "gold" => 1000,
                "regular" => 500,
                _ => throw new Exception("Unknown type: " + Type)
            };
        }
    }
}
