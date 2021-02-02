﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2_SubChain.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public int ChainId { get; set; }

        public int SenderId { get; set; }

        public string Filename { get; set; }

        public IFormFile File { get; set; }

        public DateTime UploadDate { get; set; }

        public DateTime? EditDate { get; set; }
    }
}
