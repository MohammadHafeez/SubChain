using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2_SubChain.Models
{
    public class ChainViewModel
    {
        public string Status { get; set; }

        public List<Users> EfficientChain { get; set; }

        public List<Users> ResponsiveChain { get; set; }

        public ChainViewModel()
        {
            EfficientChain = new List<Users>();
            ResponsiveChain = new List<Users>();
        }
    }
}
