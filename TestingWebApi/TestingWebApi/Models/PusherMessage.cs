using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestingWebApi.Models
{
    public class PusherMessage
    {
        public string message { get; set; }
        public User user { get; set; }
        public string messagea_id { get; set; }
        public MessagePayload message_payload { get; set; }
        public string eventPush { get; set; }
        public string channel { get; set; }
    }

    public class MessagePayload
    {
        public string message { get; set; }
        public ParsePointer user { get; set; }
        public string thread_id { get; set; }
        public string objectId { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }
    public class User
    {
        public string display_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public int user_state { get; set; }
        public string username { get; set; }
        public string objectId { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }
}