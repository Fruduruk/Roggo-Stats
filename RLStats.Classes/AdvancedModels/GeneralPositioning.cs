﻿using System;

namespace RLStats_Classes.AdvancedModels
{
    public class GeneralPositioning
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public float? Time_defensive_third { get; set; }
        public float? Time_neutral_third { get; set; }
        public float? Time_offensive_third { get; set; }
        public float? Time_defensive_half { get; set; }
        public float? Time_offensive_half { get; set; }
        public float? Time_behind_ball { get; set; }
        public float? Time_infront_ball { get; set; }
    }
}