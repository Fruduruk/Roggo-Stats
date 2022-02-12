using System;

namespace RLStats_Classes.AdvancedModels
{
    public class Ball
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public float? Possession_time { get; set; }
        public float? Time_in_side { get; set; }
    }
}