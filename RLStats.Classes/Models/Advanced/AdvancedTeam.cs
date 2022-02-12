﻿
using System.Collections.Generic;

namespace RLStats_Classes.Models.Advanced
{
    public class AdvancedTeam
    {
        public string Color { get; set; }
        public List<AdvancedPlayer> Players { get; set; }
        public TeamStats Stats { get; set; }
    }
}
