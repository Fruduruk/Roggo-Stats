using System;

namespace RLStats_Classes.AdvancedModels
{
    public class Camera
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int? Fov { get; set; }
        public int? Height { get; set; }
        public int? Pitch { get; set; }
        public int? Distance { get; set; }
        public float? Stiffness { get; set; }
        public float? Swivel_speed { get; set; }
        public float? Transition_speed { get; set; }
    }
}
