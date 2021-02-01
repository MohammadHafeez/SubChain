using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2_SubChain.Models
{
    public class ChatViewModel
    {
        public int ChatId { get; set; }

        public Users User1 { get; set; }

        public Users User2 { get; set; }

        public List<Messages> Messages { get; set; }

        public ChatViewModel()
        {
            Messages = new List<Messages>();
        }
    }
}
