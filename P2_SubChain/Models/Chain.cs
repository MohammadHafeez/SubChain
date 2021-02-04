using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2_SubChain.Models
{
    public class Chain
    {
        public int ChainId { get; set; }

        public int ChainOwner { get; set; }

        public string EfficientChain { get; set; }

        public string ResponsiveChain { get; set; }

        public string Status { get; set; }
    }
}
