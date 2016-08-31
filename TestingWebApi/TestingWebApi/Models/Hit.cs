using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestingWebApi.Models
{
    public class Hit
    {
        public string type { get; set; }
        public bool IsPayment { get; set; }
        public string Object { get; set; }
    }
}