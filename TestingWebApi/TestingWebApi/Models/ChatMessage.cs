using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestingWebApi.Models
{
    public class ChatMessage
    {
        public string message { get; set; }
        public string messagea_id { get; set; }
        public string eventPush { get; set; }
        public string channel { get; set; }
        public string userId { get; set; }
    }
}