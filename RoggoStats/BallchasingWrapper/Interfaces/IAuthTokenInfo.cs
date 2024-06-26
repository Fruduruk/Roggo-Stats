﻿namespace BallchasingWrapper.Interfaces
{
    public interface IAuthTokenInfo
    {
        public string Token { get; set; }
        public string Type { get; set; }
        /// <summary>
        /// Returns speed measured in calls per second
        /// </summary>
        /// <returns></returns>
        public double GetSpeed();

        /// <summary>
        /// returns hour limit measured in calls per hour
        /// </summary>
        /// <returns></returns>
        public double GetHourLimit();
    }
}
