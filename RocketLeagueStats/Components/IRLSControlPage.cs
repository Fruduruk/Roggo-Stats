﻿using RocketLeagueStats.AdvancedModels;
using System;
using System.Collections.Generic;

namespace RocketLeagueStats.Components
{
    public interface IRLSControlPage
    {
        public event EventHandler<string> NotificationMessageTriggered;
        public AdvancedLogic Logic { get; set; }
        public List<AdvancedReplay> AdvancedReplays { get; set; }
    }
}