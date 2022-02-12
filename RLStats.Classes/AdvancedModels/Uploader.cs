using System;

namespace RLStats_Classes.AdvancedModels
{
    public class Uploader
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Steam_id { get; set; }
        public string Name { get; set; }
        public string Profile_url { get; set; }
        public string Avatar { get; set; }
    }
}