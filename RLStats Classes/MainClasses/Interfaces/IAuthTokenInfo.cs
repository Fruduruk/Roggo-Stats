using System.Collections.Generic;

namespace RLStats_Classes.MainClasses.Interfaces
{
    public interface IAuthTokenInfo
    {
        public string Token { get; }
        /// <summary>
        /// Returns speed measured in calls per second
        /// </summary>
        /// <returns></returns>
        public int GetSpeed();

        /// <summary>
        /// returns hour limit measured in calls per hour
        /// </summary>
        /// <returns></returns>
        public int GetHourLimit();
    }
}
