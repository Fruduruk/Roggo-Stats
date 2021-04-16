using RLStats_Classes.AdvancedModels;
using System;
using System.Collections.Generic;

namespace RocketLeagueStats.Components
{
    public interface IRLSControlPage
    {
        public event EventHandler<string> NotificationMessageTriggered;
        public List<AdvancedReplay> AdvancedReplays { get; set; }
    }
}