using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RLStats_Classes.Models
{
    public class CollectReplaysResponse
    {
        public ApiDataPack DataPack { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }
}
