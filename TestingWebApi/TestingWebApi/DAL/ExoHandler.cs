using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using TestingWebApi.Common;
using TestingWebApi.Models;

namespace TestingWebApi.DAL
{
    public class ExoHandler
    {

        public const string className_User = "_User";
        public const string className_Profile = "Profile";

        public Dictionary<string, string> GetUserDetailForExoCall(string From, string To)
        {
            Dictionary<string, string> userDetailObj = new Dictionary<string, string>();
            var condition = "where={\"$or\":[{\"PhoneNumber\":\"" + From + "\"},{\"ExotelNumber\":\"" + To + "\"}]}";
            string parseUrl = "https://api.parse.com/1/users?" + condition;
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
                        var tempKey1 = obj["PhoneNumber"] != null ? obj["PhoneNumber"].ToString() : string.Empty;
                        var tempKey2 = obj["ExotelNumber"] != null ? obj["ExotelNumber"].ToString() : string.Empty;
                        var tempValue = obj["objectId"] != null ? obj["objectId"].ToString() : string.Empty;
                        if (!string.IsNullOrEmpty(tempKey1) && !userDetailObj.ContainsKey(tempKey1))
                            userDetailObj.Add(tempKey1, tempValue);
                        if (!string.IsNullOrEmpty(tempKey2) && !userDetailObj.ContainsKey(tempKey2))
                            userDetailObj.Add(tempKey2, tempValue);
                    }
                }
            }
            return userDetailObj;
        }

        public bool SaveExoObject(string CallSid, string From, string To, string Direction, string DialCallDuration, string StartTime, string EndTime, string CallType, string DialWhomNumber, string flow_id, string tenant_id, string CallFrom, string CallTo, string ForwardedFrom)
        {
            bool isSaved = false;
            ExoCall exoData = GetExoObject(CallSid, From, To, Direction, DialCallDuration, StartTime, EndTime, CallType, DialWhomNumber, flow_id, tenant_id, CallFrom, CallTo, ForwardedFrom);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var serializedExoData = js.Serialize(exoData);
            string parseUrl = "https://api.parse.com/1/classes/ExoCallRecord";
            //Post data to server.
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl, "Post");
            StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());

            requestWriter.Write(serializedExoData);
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

        public bool SaveCallHistory(string callEnd, string callStart, string exotelId, string fromUserObjId, string toUserObjId, string fromProfileObjId, string toProfileObjId, string duration, string status, string callStatus)
        {
            bool isSaved = false;
            if (string.IsNullOrEmpty(fromUserObjId))
                return false;
            CallHistory userCallHistory = GetHistoryObject(callEnd, callStart, exotelId, fromUserObjId, toUserObjId, fromProfileObjId, toProfileObjId, duration, status, callStatus);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var serializedCallHistory = js.Serialize(userCallHistory);
            string parseUrl = "https://api.parse.com/1/classes/CallHistory";

            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl, "Post");

            StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());

            requestWriter.Write(serializedCallHistory);
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

        public bool CheckProfileLinking(string fromUserId, string toUserId, out string fromProfile, out string toProfile, out bool isPriorityLinking, out bool isCallAllowed, out bool isOfflineCall, out string patientFullName,out bool isConnectionBlocked)
        {
            fromProfile = "";
            toProfile = "";
            isPriorityLinking = false;
            isCallAllowed = false;
            isOfflineCall = false;
            isConnectionBlocked = false;
            patientFullName = "";
            bool isVerifiedRelation = false;
            var callToFrom = "where={\"DoctorUserObjectId\":\"" + toUserId + "\",\"PatientUserObjectId\":\"" + fromUserId + "\"}&include=DoctorProfileObjectId,DoctorProfileObjectId.StatusPointer,PatientProfileObjectId";
            string parseUrl = "https://api.parse.com/1/classes/ProfileLink?" + callToFrom;
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
                        toProfile = obj["DoctorProfileObjectId"] != null ? obj["DoctorProfileObjectId"]["objectId"].ToString() : string.Empty;
                        fromProfile = obj["PatientProfileObjectId"] != null ? obj["PatientProfileObjectId"]["objectId"].ToString() : string.Empty;
                        isPriorityLinking = obj["IsPriority"] != null ? Convert.ToBoolean(obj["IsPriority"].ToString()) : false;
                        isVerifiedRelation = obj["IsVerfied"] != null ? Convert.ToBoolean(obj["IsVerfied"].ToString()) : false;
                        isConnectionBlocked = obj["IsArchive"] != null ? Convert.ToBoolean(obj["IsArchive"].ToString()) : false;
                        patientFullName = obj["PatientProfileObjectId"] != null ? obj["PatientProfileObjectId"]["FullName"].ToString() : string.Empty;
                        if (obj["DoctorProfileObjectId"] != null && obj["DoctorProfileObjectId"]["StatusPointer"] != null)
                        {
                            isCallAllowed = Convert.ToBoolean(obj["DoctorProfileObjectId"]["StatusPointer"]["IsCallAllowed"].ToString());
                        }
                        else
                        {
                            isCallAllowed = false;
                        }
                        if (obj["DoctorProfileObjectId"] != null)
                        {
                            isOfflineCall = obj["DoctorProfileObjectId"]["IsOfflineCall"]!=null?Convert.ToBoolean(obj["DoctorProfileObjectId"]["IsOfflineCall"].ToString()):false;
                        }
                        break;
                        //If priority customer skip user availability.                        
                    }
                }
            }
            if (!string.IsNullOrEmpty(fromProfile) && !string.IsNullOrEmpty(toProfile) && isVerifiedRelation)
                return true;
            return false;
        }
        public bool CheckUserAvailibility(string userObjectId, out string userStatus)
        {
            userStatus = string.Empty;
            var condition = "?where={\"UserId\":\"" + userObjectId + "\"}";
            string parseUrl = "https://api.parse.com/1/classes/UserStatus" + condition;
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
                        userStatus = obj["StatusName"] != null ? obj["StatusName"].ToString() : string.Empty;
                        break;
                    }
                }

            }
            if (userStatus.ToLower().Equals("online"))
                return true;
            else
                return false;
        }
        public bool checkUserWalletBalance(string userObjectId)
        {
            string condition = "?where={\"UserObjectId\":\"" + userObjectId + "\"}";
            string parseUrl = "https://api.parse.com/1/classes/Wallet";
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl + condition, "Get");
            decimal amount = 0;

            var httpResponseUser = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponseUser.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
                if (jObjRes["results"] != null)
                {
                    foreach (var obj in jObjRes["results"].Children())
                    {
                        try
                        {
                            amount = obj["Amount"] != null ? decimal.Parse(obj["Amount"].ToString()) : 0;
                        }
                        catch (Exception ex)
                        {
                            amount = 0;
                        }
                        break;
                    }
                }
            }
            if (amount < 0)
                return false;
            return true;
        }
        public string GetDialerNumber(string dialedNumber)
        {
            string result = string.Empty;
            string condition = "?where={\"ExotelNumber\":\"" + dialedNumber + "\"}&keys=PhoneNumber,ExotelNumber";
            string parseUrl = "https://api.parse.com/1/users";
            //Post data to server.
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl + condition, "Get");
            var httpResponseUser = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponseUser.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
                if (jObjRes["results"] != null && jObjRes["results"][0] != null)
                {
                    var tempObj = jObjRes["results"][0];
                    result = tempObj["PhoneNumber"] != null ? tempObj["PhoneNumber"].ToString() : string.Empty;
                }
            }
            return result;
        }
        public bool UpdateCallHistory(string callSid, string end, string start, string duration, out string fromUserObjectPt, out string toUserObjectPt, out string fromProfileObjectPt, out string toProfileObjectPt)
        {
            //update startTime,endTime,duration,
            bool updateHistoryObj = false;
            bool isHistoryUpdated = false;
            string callHistoryId = string.Empty;
            fromProfileObjectPt = string.Empty; fromUserObjectPt = string.Empty; toUserObjectPt = string.Empty;
            toProfileObjectPt = string.Empty;
            string condition = "?where={\"ExotelId\":\"" + callSid + "\"}";
            //:&keys=ExotelId,FromUserObjectId,ToUserObjectId,FromProfileObjectId,toProfileObjectId";
            string parseUrl = "https://api.parse.com/1/classes/CallHistory";
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl + condition, "Get");

            var httpResponseUser = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponseUser.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
                if (jObjRes["results"] != null && jObjRes["results"][0] != null)
                {
                    var tempObj = jObjRes["results"][0];
                    callHistoryId = tempObj["objectId"] != null ? tempObj["objectId"].ToString() : string.Empty;
                    fromUserObjectPt = tempObj["FromUserObjectId"] != null ? tempObj["FromUserObjectId"].ToString() : string.Empty;
                    toUserObjectPt = tempObj["ToUserObjectId"] != null ? tempObj["ToUserObjectId"].ToString() : string.Empty;
                    fromProfileObjectPt = tempObj["FromProfileObjectId"] != null ? tempObj["FromProfileObjectId"].ToString() : string.Empty;
                    toProfileObjectPt = tempObj["ToProfileObjectId"] != null ? tempObj["ToProfileObjectId"].ToString() : string.Empty;
                    if (!string.IsNullOrEmpty(callHistoryId))
                        updateHistoryObj = true;
                }
            }
            if (updateHistoryObj)
            {
                var updateData = "{\"CallEnd\":\"" + start + "\",\"Duration\":\"" + duration + "\"}";//During this leg start time is call end time.
                var httpWebRequestHistory = CommonObjects.CreateHttpRequestObject(parseUrl + "/" + callHistoryId, "Put");
                StreamWriter requestWriter = new StreamWriter(httpWebRequestHistory.GetRequestStream());
                requestWriter.Write(updateData);
                requestWriter.Close();
                var httpResponseHistory = (HttpWebResponse)httpWebRequestHistory.GetResponse();
                using (var streamReader = new StreamReader(httpResponseHistory.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    JObject jObjRes = JObject.Parse(responseText);
                    if (jObjRes["updatedAt"] != null)
                        isHistoryUpdated = true;
                }
            }
            return isHistoryUpdated;
        }

        public bool sendPushNotificationForCall(string name, string toObjectId)
        {
            string parseUrl = "https://api.parse.com/1/push";
            //Post data to server.
            var msg = name + " is calling you.";
            var data = "{\"where\": {\"UserPointer\":{\"__type\": \"Pointer\",\"className\": \"_User\",\"objectId\": \"" + toObjectId + "\"},\"Active\":true},\"data\": {\"alert\": \"" + msg + "\",\"type\":2}}";
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl, "Post");
            StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());

            requestWriter.Write(data);
            requestWriter.Close();
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
            }

            return true;
        }
        public bool SendPushNotification(string msg, string userObjectId)//This method can be used for sending push in any case.
        {
            string parseUrl = "https://api.parse.com/1/push";
            //Post data to server.            
            var data = "{\"where\": {\"UserPointer\":{\"__type\": \"Pointer\",\"className\": \"_User\",\"objectId\": \"" + userObjectId + "\"},\"Active\":true},\"data\": {\"alert\": \"" + msg + "\"}}";
            var httpWebRequest = CommonObjects.CreateHttpRequestObject(parseUrl, "Post");
            StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());

            requestWriter.Write(data);
            requestWriter.Close();
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
            }
            return true;
        }
        public bool AddMissedCallChannel(string toUserId, string fromUserId)
        {
            string parseUrl = "https://api.parse.com/1/users/";
            string missedChannel = toUserId + "-missed";
            bool missedChannelUpdated=false;
            var updateData = "{\"PushChannels\":{\"__op\":\"AddUnique\",\"objects\":[\""+ missedChannel + "\"]}}";//Adding unique missed call channel.
            var httpWebRequest = CommonObjects.CreateMasterRequestObject(parseUrl + "/" + fromUserId, "Put");
            StreamWriter requestWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            requestWriter.Write(updateData);
            requestWriter.Close();
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                JObject jObjRes = JObject.Parse(responseText);
                if (jObjRes["updatedAt"] != null)
                    missedChannelUpdated = true;
            }
            return missedChannelUpdated;
        }

        private ExoCall GetExoObject(string CallSid, string From, string To, string Direction, string DialCallDuration, string StartTime, string EndTime, string CallType, string DialWhomNumber, string flow_id, string tenant_id, string CallFrom, string CallTo, string ForwardedFrom)
        {
            ExoCall exo = new ExoCall();
            exo.CallSid = CallSid;
            exo.From = From;
            exo.To = To;
            exo.Direction = Direction;
            exo.DialCallDuration = DialCallDuration;
            exo.StartTime = StartTime;
            exo.EndTime = EndTime;
            exo.CallType = CallType;
            exo.DialWhomNumber = DialWhomNumber;
            exo.flow_id = flow_id;
            exo.tenant_id = tenant_id;
            exo.CallFrom = CallFrom;
            exo.CallTo = CallTo;
            exo.ForwardedFrom = ForwardedFrom;
            return exo;
        }

        private CallHistory GetHistoryObject(string callEnd, string callStart, string exotelId, string fromUserObjId, string toUserObjId, string fromProfileObjId, string toProfileObjId, string duration, string status, string statusType)
        {
            int type = 0;
            Int32.TryParse(statusType, out type);
            CallHistory historyObj = new CallHistory();
            historyObj.CallEnd = callEnd;
            historyObj.CallStart = callStart;
            historyObj.ExotelId = exotelId;
            historyObj.FromUserObjectId = string.IsNullOrEmpty(fromUserObjId) ? null : GetParseObject(className_User, fromUserObjId);
            historyObj.ToUserObjectId = string.IsNullOrEmpty(toUserObjId) ? null : GetParseObject(className_User, toUserObjId);
            historyObj.FromProfileObjectId = string.IsNullOrEmpty(fromProfileObjId) ? null : GetParseObject(className_Profile, fromProfileObjId);
            historyObj.ToProfileObjectId = string.IsNullOrEmpty(toProfileObjId) ? null : GetParseObject(className_Profile, toProfileObjId);
            historyObj.Duration = duration;
            historyObj.CallStatus = status;
            historyObj.CallStatusType = type;
            return historyObj;
        }

        private ParsePointer GetParseObject(string className, string objId)
        {
            ParsePointer obj = new ParsePointer();
            obj.__type = "Pointer";
            obj.className = className;
            obj.objectId = objId;
            return obj;
        }

    }
}