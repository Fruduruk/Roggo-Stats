using System;

namespace RLStats_Classes.AdvancedModels
{
    public class AdvancedPlayer
    {
        public string CustomId { get; set; } = Guid.NewGuid().ToString();
        public int? Start_time { get; set; }
        public float? End_time { get; set; }
        public string Name { get; set; }
        public Id Id { get; set; }
        public int? Car_id { get; set; }
        public string Car_name { get; set; }
        public Camera Camera { get; set; }
        public int? Steering_sensitivity { get; set; }
        public PlayerStats Stats { get; set; }
    }
}
