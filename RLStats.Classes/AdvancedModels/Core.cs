﻿using System;

namespace RLStats_Classes.AdvancedModels
{
    public class Core
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int? Shots { get; set; }
        public int? Shots_against { get; set; }
        public int? Goals { get; set; }
        public int? Goals_against { get; set; }
        public int? Saves { get; set; }
        public int? Assists { get; set; }
        public int? Score { get; set; }
        public float? Shooting_percentage { get; set; }
    }
}