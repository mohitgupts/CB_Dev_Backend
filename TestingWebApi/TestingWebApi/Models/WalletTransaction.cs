using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestingWebApi.Models
{
    public class WalletTransaction
    {
        public decimal Amount { get; set; }
        public ParsePointer FromUserObjectId { get; set; }
        public ParsePointer ToUserObjectId { get; set; }
        public ParsePointer FromProfileObjectId { get; set; }
        public ParsePointer ToProfileObjectId { get; set; }
        public ParsePointer TransTypePointer { get; set; }
        public string ExotelId { get; set; }
    }
}