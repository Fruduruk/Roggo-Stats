using System;
using System.Collections.Generic;

namespace RLStats_Classes.AdvancedModels
{
    public class AdvancedTeam
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Color { get; set; }
        public List<AdvancedPlayer> Players { get; set; }
        public TeamStats Stats { get; set; }
    }
}

