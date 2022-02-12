using System;

namespace RLStats_Classes.AdvancedModels
{
    public class Id
    {
        public string CustomId { get;set; } = Guid.NewGuid().ToString();
        public string Platform { get; set; }
        public string ID { get; set; }
    }
}