using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestingWebApi.Models
{
    public class ExoCall
    {
        public string CallSid { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Direction { get; set; }
        public string DialCallDuration { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CallType { get; set; }
        public string DialWhomNumber { get; set; }
        public string flow_id { get; set; }
        public string tenant_id { get; set; }
        public string CallFrom { get; set; }
        public string CallTo { get; set; }
        public string ForwardedFrom { get; set; }        
    }
}