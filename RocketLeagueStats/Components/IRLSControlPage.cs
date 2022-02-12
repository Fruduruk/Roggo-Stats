using RLStats_Classes.Models.Advanced;

using System;
using System.Collections.Generic;

namespace RocketLeagueStats.Components
{
    public interface IRLSControlPage
    {
        public event EventHandler<string> NotificationMessageTriggered;
        public List<AdvancedReplay> AdvancedReplays { get; set; }
        public List<AdvancedReplay> AdvancedReplaysToCompare { get; set; }
    }
}