using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2_SubChain.Models
{
    public class AddIntermediaryViewModel
    {
        public List<Users> UserList { set; get; }

        public string ChainType { get; set; }

        public AddIntermediaryViewModel()
        {
            UserList = new List<Users>();
        }
    }
}
