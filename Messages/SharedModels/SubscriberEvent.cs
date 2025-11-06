using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages.SharedModels
{
    public class SubscriberEvent
    {
        public string Username { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = null!;
    }
}
