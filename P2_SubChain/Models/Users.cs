using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2_SubChain.Models
{
    public class Users
    {
        public int UserId { get; set; }

        public string CompanyName { get; set; }

        public string CompanyType { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Country { get; set; }

        public string CityorState { get; set; }

        public string ProductCategoryString { get; set; }

        public List<string> ProductCategory { get; set; }
    }
}
