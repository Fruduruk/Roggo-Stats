using System;

namespace RLStats_Classes.AdvancedModels
{
    public class TeamStats
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public Ball Ball { get; set; }
        public Core Core { get; set; }
        public Boost Boost { get; set; }
        public GeneralMovement Movement { get; set; }
        public GeneralPositioning Positioning { get; set; }
        public Demo Demo { get; set; }
    }
}