using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2_SubChain.Models
{
    public class Invoice
    {
        public int DistributorID { get; set; }

        public int SupplierID { get; set; }

        public DateTime IDate { get; set; }
    }
}
