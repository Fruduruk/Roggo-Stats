using RLStats_Classes.MainClasses;

using System;
using System.Collections.Generic;

namespace RLStats_Classes.Models
{
    public class ApiDataPack
    {
        public List<Replay> Replays { get; set; }
        public int ReplayCount { get; set; }
        public string Next { get; set; }
        public bool Success { get; set; }
        public string ReceivedString { get; set; } = string.Empty;
    }
}
