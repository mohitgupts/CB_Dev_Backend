using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;
using TestingWebApi.Common;
using TestingWebApi.DAL;
using TestingWebApi.Models;

namespace TestingWebApi.Controllers
{
    [RoutePrefix("Telephony")]
    public class ExoController : ApiController
    {
        [HttpGet]
        [Route("ExoCallStart")]
        public HttpResponseMessage ExoCallStartRecord(string CallSid, string From, string To, string Direction, string StartTime, string EndTime, string CallType, string flow_id, string tenant_id, string CallFrom, string CallTo, string DialCallDuration)
        {
            Dictionary<string, string> userDetailObject = new Dictionary<string, string>();
            ExoHandler handlerObj = new ExoHandler();
            string toProfileObjectId = string.Empty, fromProfileObjectId = string.Empty;
            string dialerNumber = From.TrimStart('0');
            string dialedTo = To.TrimStart('0');
            bool isPriority = false;
            bool isCallAllowed = false;
            bool isOfflineCallAllowed = false;
            bool isConnectionBlocked = false;
            string callerFullName;
            ExoResult exoResultData = new ExoResult();
            HttpResponseMessage response;
            response = Request.CreateResponse(HttpStatusCode.NotFound, "");
            string callStatus = "";
            //optional parameters changes
            string DialWhomNumber ="0";
            string ForwardedFrom = "0123456789";

            //Saving exotel response for call start.
            handlerObj.SaveExoObject(CallSid, From, To, Direction, DialCallDuration, StartTime, EndTime, CallType, DialWhomNumber, flow_id, tenant_id, CallFrom, CallTo, ForwardedFrom);

            //Fetching Dialer & Dialed number details.
            userDetailObject = handlerObj.GetUserDetailForExoCall(dialerNumber, dialedTo);
            if (userDetailObject.ContainsKey(dialerNumber) && userDetailObject.ContainsKey(dialedTo))
            {
                //Verifying Dialer user & Dialed user connection.
                if (!handlerObj.CheckProfileLinking(userDetailObject[dialerNumber], userDetailObject[dialedTo], out fromProfileObjectId, out toProfileObjectId, out isPriority, out isCallAllowed, out isOfflineCallAllowed, out callerFullName, out isConnectionBlocked))
                {
                    exoResultData.select = "2";//Means user is not connected with doc.
                }
                else
                {
                    if (isConnectionBlocked)//Doc archived this patient & hence call can't be proceded.
                    {
                        exoResultData.select = "8";//Call blocked.
                        callStatus = "blocked";
                    }
                    else if ((isOfflineCallAllowed && isPriority) || isCallAllowed)
                    {
                        if (handlerObj.checkUserWalletBalance(userDetailObject[dialerNumber]))
                        {
                            exoResultData.select = "1";//Finally everything works. Hurray Let's forward call.
                            callStatus = "connected";
                            handlerObj.sendPushNotificationForCall(callerFullName, userDetailObject[dialedTo]);
                        }
                        else
                        {
                            exoResultData.select = "4";//Means user don't have enough balance
                            callStatus = "low balance";
                            handlerObj.SendPushNotification("Your balance is low. Please add credits to your Health Account to make calls", userDetailObject[dialerNumber]);
                        }
                    }
                    else
                    {
                        exoResultData.select = "3";//Means user is offline
                        callStatus = "offline";
                        handlerObj.SendPushNotification("Doctor is offline. Please try chatting meanwhile and we will notify you once Doctor is back online", userDetailObject[dialerNumber]);
                        handlerObj.AddMissedCallChannel(userDetailObject[dialedTo], userDetailObject[dialerNumber]);//Adding missed call channel.
                    }
                    handlerObj.SaveCallHistory(EndTime, StartTime, CallSid, userDetailObject.ContainsKey(dialerNumber) ? userDetailObject[dialerNumber] : "", userDetailObject.ContainsKey(dialedTo) ? userDetailObject[dialedTo] : "", fromProfileObjectId, toProfileObjectId, DialCallDuration, callStatus, exoResultData.select);
                }
            }
            else if (!userDetailObject.ContainsKey(dialerNumber))
            {
                exoResultData.select = "5";//Means..unregistered number..App ka link bhejo re..!!                
            }
            else if (!userDetailObject.ContainsKey(dialedTo))
            {
                exoResultData.select = "6";//Means..unregistered exotel number..kuch to gadbad h..!!                
            }
            else
            {
                exoResultData.select = "7";//Means..Galti se aa gya ree..!!                
            }
            response = Request.CreateResponse<ExoResult>(HttpStatusCode.OK, exoResultData);


            return response;
        }

        [HttpGet]
        [Route("DialWhomNumber")]
        public HttpResponseMessage GetDialedOriginalNumber(string To)//string CallSid, string From, string To, string CallStatus, string Direction)
        {
            HttpResponseMessage response;
            string originalNumber = string.Empty;
            ExoHandler handlerObj = new ExoHandler();
            string dialerNumber = To.TrimStart('0');
            originalNumber = handlerObj.GetDialerNumber(dialerNumber);//To);
            response = Request.CreateResponse<string>(HttpStatusCode.OK, originalNumber);
            return response;
        }

        [HttpGet]
        [Route("ExoCallEnd")]
        //public HttpResponseMessage ExoCallEndRecord(string CallSid, string From, string To, string Direction, string DialCallDuration, string StartTime, string EndTime, string CallType, string DialWhomNumber, string flow_id, string tenant_id, string CallFrom, string CallTo, string ForwardedFrom)
        public HttpResponseMessage ExoCallEndRecord(string CallSid, string From, string To, string Direction, string StartTime, string EndTime, string CallType, string flow_id, string tenant_id, string CallFrom, string CallTo, string DialCallDuration)
        {
            HttpResponseMessage response;            
            ExoHandler handlerObj = new ExoHandler();
            int callDuration = 0;
            //optional parameters changes
            string DialWhomNumber = "0";
            string ForwardedFrom = "0123456789";

            Int32.TryParse(DialCallDuration, out callDuration);
            string strFromUserObjId, strToUserObjId, strFromProfileObjId, strToProfileObjId;
            handlerObj.SaveExoObject(CallSid, From, To, Direction, DialCallDuration, StartTime, EndTime, CallType, DialWhomNumber, flow_id, tenant_id, CallFrom, CallTo, ForwardedFrom);
            //Update call history
            handlerObj.UpdateCallHistory(CallSid, EndTime, StartTime, DialCallDuration, out strFromUserObjId, out strToUserObjId, out strFromProfileObjId, out strToProfileObjId);
            //If duration is 0, then send push to patient. and don't make any transaction history object.
            if (callDuration <= 0)
            {
                handlerObj.SendPushNotification("Doctor is busy. Please try chatting meanwhile.", GetObjecIdFromPointer(strFromUserObjId));
                //handlerObj.AddMissedCallChannel(GetObjecIdFromPointer(strToUserObjId), GetObjecIdFromPointer(strFromUserObjId));//Adding missed call channel.
            }
            else
            {
                //Finally Transaction and call finish.
                MakeCallPayment(DialCallDuration, CallSid, strFromProfileObjId, strToProfileObjId, strFromUserObjId, strToUserObjId);
            }

            response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        public bool MakeCallPayment(string duration, string exotelId, string fromProfileStr, string toProfileStr, string fromUserStr, string toUserStr)
        {

            //Calculate the call charges.
            bool isTransactionSaved = false;
            decimal callBill = 0;
            string fromUserObjId = GetObjecIdFromPointer(fromUserStr);
            string toUserObjectId = GetObjecIdFromPointer(toUserStr);
            callBill = GetCallCharges(fromProfileStr, toProfileStr, duration);

            WalletTransaction transaction = GetTransactionObject(callBill, fromProfileStr, toProfileStr, fromUserStr, toUserStr, exotelId);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            string serializedTransactionData = ser.Serialize(transaction);

            Dictionary<string, string> walletIds = GetUserWallet(fromUserObjId, toUserObjectId);
            if (walletIds.ContainsKey(fromUserObjId) && walletIds.ContainsKey(toUserObjectId))
            {
                var batchUrl = "https://api.parse.com/1/batch";
                var paymentBatchRequest = "{\"requests\": [{\"method\": \"POST\",\"path\": \"/1/classes/TransactionHistory\",\"body\":" + serializedTransactionData + "},{\"method\": \"PUT\",\"path\": \"/1/classes/Wallet/" + walletIds[fromUserObjId] + "\",\"body\": {\"Amount\":{\"__op\": \"Increment\",\"amount\":" + ((-1) * callBill) + "}}},{\"method\": \"PUT\",\"path\": \"/1/classes/Wallet/" + walletIds[toUserObjectId] + "\",\"body\": {\"Amount\":{\"__op\": \"Increment\",\"amount\":" + (callBill) + "}}}]}";
                var httpWebRequest = CommonObjects.CreateHttpRequestObject(batchUrl, "Post");
                StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                requestWriter.Write(paymentBatchRequest);
                requestWriter.Close();
                var httpResponseUser = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponseUser.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    isTransactionSaved = true;
                }
            }
            return isTransactionSaved;
        }
        public Dictionary<string, string> GetUserWallet(string fromUser, string toUser)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string condition = "?where={\"$or\":[{\"UserObjectId\":\"" + fromUser + "\"},{\"UserObjectId\":\"" + toUser + "\"}]}";
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
        public decimal GetCallCharges(string fromProfile, string toProfile, string duration)
        {
            var condition = "/" + GetObjecIdFromPointer(toProfile);
            decimal fixedCharge = 0, chargesPerMin = 0;
            //int forMin = 0;
            decimal bill = 0;
            string parseUrl = "https://api.parse.com/1/classes/Profile" + condition;
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl + condition, "Get");

            var httpResponseUser = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponseUser.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);

                if (jObjRes["FirstMin"] != null)
                    Decimal.TryParse(jObjRes["FirstMin"].ToString(), out fixedCharge);
                //if (jObjRes["ForMins"] != null)
                //    Int32.TryParse(jObjRes["ForMins"].ToString(), out forMin);
                if (jObjRes["ChargesPerMin"] != null)
                    Decimal.TryParse(jObjRes["ChargesPerMin"].ToString(), out chargesPerMin);
            }
            if (fixedCharge <= 0 && chargesPerMin <= 0)
                return bill;
            else
            {
                int seconds = Convert.ToInt32(duration);
                int mins = (seconds / 60) + ((seconds % 60) > 0 ? 1 : 0);
                bill = mins * chargesPerMin;//calculated per min charges.
                bill = fixedCharge > bill ? fixedCharge : bill;//considering maximum of fixed charge or per min charges.
                return bill;
            }
        }
        private WalletTransaction GetTransactionObject(decimal amount, string fromProfile, string toProfile, string fromuser, string toUser, string exotelId)
        {
            WalletTransaction transaction = new WalletTransaction();
            transaction.Amount = amount;
            transaction.FromUserObjectId = GetPointerObjectFromJson(fromuser);
            transaction.ToUserObjectId = GetPointerObjectFromJson(toUser);
            transaction.FromProfileObjectId = GetPointerObjectFromJson(fromProfile);
            transaction.ToProfileObjectId = GetPointerObjectFromJson(toProfile);
            transaction.TransTypePointer = GetParseObject("TransactionType", "TeFUqPRVZ2");//Always fixed for ByCall object id.
            transaction.ExotelId = exotelId;
            return transaction;
        }
        private string GetObjecIdFromPointer(string pointer)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var modifiedStr = "[" + pointer.Replace("__type", "type") + "]";
            List<DummyPointer> obj = ser.Deserialize<List<DummyPointer>>(modifiedStr);

            return obj[0].objectId;
        }
        private ParsePointer GetParseObject(string className, string objId)
        {
            ParsePointer obj = new ParsePointer();
            obj.__type = "Pointer";
            obj.className = className;
            obj.objectId = objId;
            return obj;
        }
        private ParsePointer GetPointerObjectFromJson(string json)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var modifiedStr = "[" + json.Replace("__type", "type") + "]";
            List<DummyPointer> obj = ser.Deserialize<List<DummyPointer>>(modifiedStr);
            return GetParseObject(obj[0].className, obj[0].objectId);
        }


    }
}