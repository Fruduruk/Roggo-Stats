using System;
using System.Collections.Generic;
using System.Text;

namespace RocketLeagueStats.AverageModels
{
    public class AveragePlayerCore
    {
        public double? MVP { get; set; }
        public double? Shots { get; set; }
        public double? Shots_against { get; set; }
        public double? Goals { get; set; }
        public double? Goals_against { get; set; }
        public double? Saves { get; set; }
        public double? Assists { get; set; }
        public double? Score { get; set; }
        public double? Shooting_percentage { get; set; }
    }
}
