using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using TestingWebApi.Common;
using TestingWebApi.Models;

namespace TestingWebApi.DAL
{
    public class PaymentHandler
    {

        public Payment GetPaymentDetail(string tranId)
        {
            Payment paymentObject = new Payment();
            string URI = "https://www.payumoney.com/payment/op/getPaymentResponse";
            string merchantKey = System.Configuration.ConfigurationManager.AppSettings["Merchant_key"].ToString();
            string authorizationHeader = System.Configuration.ConfigurationManager.AppSettings["PayuAuth"].ToString();
            string myParameters = "merchantKey=" + merchantKey + "&merchantTransactionIds=" + tranId;
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                wc.Headers.Add("Authorization", authorizationHeader);
                try
                {
                    string HtmlResult = wc.UploadString(URI, myParameters);
                    JObject jObjRes = JObject.Parse(HtmlResult);
                    if (jObjRes["result"] != null && jObjRes["result"].Count() > 0 && jObjRes["result"][0]["postBackParam"] != null)
                    {
                        JToken response = jObjRes["result"][0]["postBackParam"];
                        paymentObject = CreatePaymentObject(response);
                        return paymentObject;
                    }
                }
                catch (Exception ex)
                {
                }

            }
            return null;
        }

        public bool SavePaymentObject(Payment payment)
        {
            bool isSaved = false;
            JavaScriptSerializer js = new JavaScriptSerializer();
            var serializePaymentObject = js.Serialize(payment);
            string parseUrl = "https://api.parse.com/1/classes/PaymentRecord";
            //Post data to server.
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl, "Post");
            StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());

            requestWriter.Write(serializePaymentObject);
            requestWriter.Close();
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
                if (jObjRes["objectId"] != null)
                    isSaved = true;

            }
            return isSaved;
        }
        public bool CheckTransactionDuplicate(string paymentId)
        {
            bool result = false;
            var condition = "?where={\"ExotelId\":\"" + paymentId + "\"}";
            string parseUrl = "https://api.parse.com/1/classes/TransactionHistory" + condition;
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl, "Get");
            var httpResponseUser = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponseUser.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
                if (jObjRes["results"] != null)
                {
                    foreach (var obj in jObjRes["results"].Children())
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public bool AddPayment(decimal amount, string userId, string payuId)
        {
            bool isSaved = false;
            Dictionary<string, string> userWalletId = GetUserWallet(userId);
            string profileObjId = GetProfileId(userId);
            if (profileObjId != null && profileObjId.Trim() != "")
            {
                WalletTransaction transaction = GetTransactionObject(amount, profileObjId, userId, payuId);
                JavaScriptSerializer ser = new JavaScriptSerializer();
                string serializedTransactionData = ser.Serialize(transaction);
                if (userWalletId.ContainsKey(userId) && amount > 0)
                {
                    var batchUrl = "https://api.parse.com/1/batch";
                    var paymentBatchRequest = "{\"requests\": [{\"method\": \"POST\",\"path\": \"/1/classes/TransactionHistory\",\"body\":" + serializedTransactionData + "},{\"method\": \"PUT\",\"path\": \"/1/classes/Wallet/" + userWalletId[userId] + "\",\"body\": {\"Amount\":{\"__op\": \"Increment\",\"amount\":" + amount + "}}}]}";
                    var httpWebRequest = CommonObjects.CreateHttpRequestObject(batchUrl, "Post");
                    StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                    requestWriter.Write(paymentBatchRequest);
                    requestWriter.Close();
                    var httpResponseUser = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponseUser.GetResponseStream()))
                    {
                        var responseText = streamReader.ReadToEnd();
                        isSaved = true;
                    }
                }
            }
            return isSaved;
        }
        public string GetProfileId(string userId)
        {
            string defaultProfileObjId = "";
            var condition = "?where={\"UserObjectId\":\"" + userId + "\",\"IsParent\":true}";
            string parseUrl = "https://api.parse.com/1/classes/Profile" + condition;
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl, "Get");
            var httpResponseUser = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponseUser.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
                if (jObjRes["results"] != null)
                {
                    foreach (var obj in jObjRes["results"].Children())
                    {
                        defaultProfileObjId = obj["objectId"] != null ? obj["objectId"].ToString() : string.Empty;
                        break;
                    }
                }

            }

            return defaultProfileObjId;
        }
        public Dictionary<string, string> GetUserWallet(string user)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string condition = "?where={\"UserObjectId\":\"" + user + "\"}";
            string parseUrl = "https://api.parse.com/1/classes/Wallet";
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl + condition, "Get");

            var httpResponseUser = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponseUser.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
                if (jObjRes["results"] != null)
                {
                    foreach (var obj in jObjRes["results"].Children())
                    {
                        var key = obj["UserObjectId"] != null ? obj["UserObjectId"].ToString() : string.Empty;
                        var value = obj["objectId"] != null ? obj["objectId"].ToString() : string.Empty;
                        if (!string.IsNullOrEmpty(key) && !result.ContainsKey(key))
                            result.Add(key, value);
                    }
                }
            }
            return result;
        }
        private WalletTransaction GetTransactionObject(decimal amount, string toProfile, string toUser, string paymentId)
        {
            WalletTransaction transaction = new WalletTransaction();
            transaction.Amount = amount;
            transaction.ToUserObjectId = GetParseObject("_User", toUser);
            //transaction.FromProfileObjectId = GetParseObject("Profile", toProfile);
            transaction.ToProfileObjectId = GetParseObject("Profile", toProfile);
            transaction.TransTypePointer = GetParseObject("TransactionType", "08WsFYca3O");//Always fixed for payment through Payu object id.
            transaction.ExotelId = paymentId;
            return transaction;
        }
        private Payment CreatePaymentObject(JToken obj)
        {
            Payment payment = new Payment();
            payment.paymentId = obj["paymentId"] != null ? obj["paymentId"].ToString() : "";
            payment.merchantTransactionId = obj["txnid"] != null ? obj["txnid"].ToString() : "";
            payment.amount = obj["amount"] != null ? obj["amount"].ToString() : "";
            payment.status = obj["status"] != null ? obj["status"].ToString() : "";
            payment.paymentMode = obj["mode"] != null ? obj["mode"].ToString() : "";
            payment.customerEmail = obj["email"] != null ? obj["email"].ToString() : "";
            payment.customerPhone = obj["phone"] != null ? obj["phone"].ToString() : "";
            payment.customerName = obj["firstname"] != null ? obj["firstname"].ToString() : "";
            payment.udf1 = obj["udf1"] != null ? obj["udf1"].ToString() : "";
            payment.udf2 = obj["udf2"] != null ? obj["udf2"].ToString() : "";
            payment.udf3 = obj["udf3"] != null ? obj["udf3"].ToString() : "";
            payment.udf4 = obj["udf4"] != null ? obj["udf4"].ToString() : "";
            payment.udf5 = obj["udf5"] != null ? obj["udf5"].ToString() : "";
            payment.productInfo = obj["productinfo"] != null ? obj["productinfo"].ToString() : "";
            payment.additionalCharges = obj["additionalCharges"] != null ? obj["additionalCharges"].ToString() : "";
            payment.split_info = obj["split_info"] != null ? obj["split_info"].ToString() : "";
            payment.error_message = obj["error_message"] != null ? obj["error_message"].ToString() : "";
            payment.notificationId = obj["notificationId"] != null ? obj["notificationId"].ToString() : "";
            payment.hash = obj["hash"] != null ? obj["hash"].ToString() : "";
            payment.addedOnPayu = obj["addedon"] != null ? obj["addedon"].ToString() : "";
            payment.createdOnPayu = obj["createdOn"] != null ? obj["createdOn"].ToString() : "";
            return payment;
        }
        private ParsePointer GetParseObject(string className, string objId)
        {
            ParsePointer obj = new ParsePointer();
            obj.__type = "Pointer";
            obj.className = className;
            obj.objectId = objId;
            return obj;
        }
        public void RequestReceived(Payment payment)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            Hit dp = new Hit();
            dp.type = "Payment";
            dp.IsPayment = payment != null ? true : false;
            dp.Object = js.Serialize(payment);
            string responseObj = js.Serialize(dp);
            string parseUrl = "https://api.parse.com/1/classes/PaymentHit";
            //Post data to server.
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl, "Post");
            StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());

            requestWriter.Write(responseObj);
            requestWriter.Close();
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
            }
        }
        public void RequestLog(string tye, string requestObject)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            Hit dp = new Hit();
            dp.type = tye;
            dp.IsPayment = requestObject != null ? true : false;
            dp.Object = js.Serialize(requestObject);
            string responseObj = js.Serialize(dp);
            string parseUrl = "https://api.parse.com/1/classes/PaymentHit";
            //Post data to server.
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl, "Post");
            StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());

            requestWriter.Write(responseObj);
            requestWriter.Close();
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
            }
        }
    }

}