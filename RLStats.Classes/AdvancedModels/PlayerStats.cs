using System;

namespace RLStats_Classes.AdvancedModels
{
    public class PlayerStats
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public PlayerCore PlayerCore { get; set; }
        public PlayerBoost PlayerBoost { get; set; }
        public PlayerMovement PlayerMovement { get; set; }
        public PlayerPositioning PlayerPositioning { get; set; }
        public Demo Demo { get; set; }
    }
}

