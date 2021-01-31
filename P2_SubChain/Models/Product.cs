using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2_SubChain.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public int UserId { get; set; }


        public string ProductDesc { get; set; }

        public string ProductName { get; set; }

        public string ProductType { get; set; }

        public double ProductPrice { get; set; }

        public string ImageUrl { get; set; }

        public IFormFile Image { get; set; }
    }
}
