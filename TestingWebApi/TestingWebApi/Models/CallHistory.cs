using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestingWebApi.Models
{
    public class CallHistory
    {
        public string CallEnd { get; set; }
        public string CallStart { get; set; }
        public string ExotelId { get; set; }
        public ParsePointer FromUserObjectId { get; set; }
        public ParsePointer ToUserObjectId { get; set; }
        public ParsePointer FromProfileObjectId { get; set; }
        public ParsePointer ToProfileObjectId { get; set; }
        public string Duration { get; set; }
        public string CallStatus { get; set; }
        public int CallStatusType { get; set; }
    }
}