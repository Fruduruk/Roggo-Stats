using RLStats_Classes.MainClasses.Interfaces;
using System;

namespace RLStats_Classes.MainClasses
{
    public class ServiceTokenInfo : IAuthTokenInfo
    {
        public string Token { get; }
        public ServiceTokenInfo(string token)
        {
            Token = token;
        }
        public int GetSpeed()
        {
            throw new NotImplementedException();
        }
        public int GetHourLimit()
        {
            throw new NotImplementedException();
        }
    }
}
