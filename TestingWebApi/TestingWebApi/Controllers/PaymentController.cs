using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TestingWebApi.DAL;

namespace TestingWebApi.Controllers
{
    [RoutePrefix("Payment")]
    public class PaymentController : ApiController
    {
        [HttpPost]
        [Route("Success")]
        public HttpResponseMessage Post([FromBody] Payment payment)
        //string PaymentId, string MerchantTransactionId, string Amount, string Status, string PaymentMode, string CustomerEmail, string CustomerPhone, string CustomerName, string udf1, string udf2, string udf3, string udf4, string udf5, string ProductInfo, string AdditionalCharges, string SplitInfo, string ErrorMessage, string NotificationId, string Hash)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Accepted);
            //bool paymentSaved = false, paymentAdded = false;
            string msg = "unsuccessful";
            decimal Amount = 0;
            PaymentHandler payHandler = new PaymentHandler();
            payHandler.RequestReceived(payment);
            //if (payment != null)
            //{
            //    paymentSaved = payHandler.SavePaymentObject(payment);
            //    if (payment.status.ToLower() == "success" && payment.udf1!=null)
            //    {
            //        Decimal.TryParse(payment.amount, out Amount);
            //        if (paymentSaved)
            //        {
            //            paymentAdded = payHandler.AddPayment(Amount, payment.udf1, payment.paymentId);
            //            if (paymentAdded)
            //                msg = "successful";
            //        }
            //    }
            //}
            response = Request.CreateResponse<string>(HttpStatusCode.OK, msg);
            return response;
        }

        [HttpGet]
        [Route("GetPaymentObject")]
        public HttpResponseMessage GetPayment(string merchantTransactionId)
        {
            HttpResponseMessage response;
            bool paymentSaved = false, paymentAdded = false;
            decimal Amount = 0;
            string msg = "unsuccessful";
            Payment paymentObj = new Payment();
            PaymentHandler payHandler = new PaymentHandler();
            paymentObj = payHandler.GetPaymentDetail(merchantTransactionId);
            if (paymentObj != null)
            {
                paymentSaved = payHandler.SavePaymentObject(paymentObj);
                if (!payHandler.CheckTransactionDuplicate(paymentObj.paymentId))
                {
                    if (paymentObj.status.ToLower() == "success" && paymentObj.udf1 != null)
                    {
                        Decimal.TryParse(paymentObj.amount, out Amount);
                        if (paymentSaved)
                        {
                            paymentAdded = payHandler.AddPayment(Amount, paymentObj.udf1, paymentObj.paymentId);
                            if (paymentAdded)
                                msg = "successful";
                        }
                    }
                }
            }
            response = Request.CreateResponse(HttpStatusCode.Found, "success");
            return response;
        }
    }
}