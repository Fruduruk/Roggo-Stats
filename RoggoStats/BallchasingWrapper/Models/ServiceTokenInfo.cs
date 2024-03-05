using System.Diagnostics;
using BallchasingWrapper.Interfaces;

namespace BallchasingWrapper.Models
{
    public class ServiceTokenInfo : IAuthTokenInfo
    {
        public string Token { get; set; }
        public string Type { get; set; }

        public ServiceTokenInfo(string token)
        {
            Token = token;
        }
        public double GetSpeed()
        {
            if (Debugger.IsAttached)
                return 16d;
            return 0.067d;
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
