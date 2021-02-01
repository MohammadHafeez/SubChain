using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2_SubChain.Models
{
    public class Messages
    {
        public int MessageId { get; set; }

        public int ChatId { get; set; }

        public int SenderId { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
