using System;

namespace RLStats_Classes.AdvancedModels
{
    public class Demo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int? Inflicted { get; set; }
        public int? Taken { get; set; }
    }
}