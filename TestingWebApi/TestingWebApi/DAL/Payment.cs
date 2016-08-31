using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestingWebApi.DAL
{
    public class Payment
    {
        public string paymentId { get; set; }
        public string merchantTransactionId { get; set; }
        public string amount { get; set; }
        public string status { get; set; }
        public string paymentMode { get; set; }
        public string customerEmail { get; set; }
        public string customerPhone { get; set; }
        public string customerName { get; set; }
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
        public string productInfo { get; set; }
        public string additionalCharges { get; set; }
        public string split_info { get; set; }
        public string error_message { get; set; }
        public string notificationId { get; set; }
        public string hash { get; set; }
        public string addedOnPayu { get; set; }
        public string createdOnPayu { get; set; } 
    }
}