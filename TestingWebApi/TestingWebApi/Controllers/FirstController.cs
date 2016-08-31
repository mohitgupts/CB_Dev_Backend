using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using TestingWebApi.Common;
using TestingWebApi.Models;

namespace TestingWebApi.Controllers
{
    [RoutePrefix("customers")]
    public class FirstController : ApiController
    {
        [HttpGet]
        [Route("BulkSms")]
        public HttpResponseMessage GetBulkSms(string To,string message)
        {
            HttpResponseMessage response;
            string from = "01133138966";            
            string[] toArray = To.Split(',');
            for(int i=0;i<toArray.Length;i++)
            {
                SendSms(from, toArray[i], message);
            }
            response = Request.CreateResponse<string>(HttpStatusCode.OK,"Successful");
            return response;
        }
        public string SendSms(string from, string to, string Body)
        {
            Dictionary<string, string> postValues = new Dictionary<string, string>();
            postValues.Add("From", from);
            postValues.Add("To", to);            
            postValues.Add("Body", Body);
            string SID = ConfigurationManager.AppSettings["SmsId"];
            string Token = ConfigurationManager.AppSettings["SmsKey"];

            String postString = "";

            foreach (KeyValuePair<string, string> postValue in postValues)
            {
                postString += postValue.Key + "=" + HttpUtility.UrlEncode(postValue.Value) + "&";
            }
            postString = postString.TrimEnd('&');
            
            ServicePointManager.ServerCertificateValidationCallback = delegate
            {
                return true;
            };
            string smsURL = "https://twilix.exotel.in/v1/Accounts/"+SID+"/Sms/send";
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(smsURL);
            objRequest.Credentials = new NetworkCredential(SID, Token);
            objRequest.Method = "POST";
            objRequest.ContentLength = postString.Length;
            objRequest.ContentType = "application/x-www-form-urlencoded";
            // post data is sent as a stream                                                                                                                        
            StreamWriter opWriter = null;
            opWriter = new StreamWriter(objRequest.GetRequestStream());
            opWriter.Write(postString);
            opWriter.Close();

            // returned values are returned as a stream, then read into a string                                                                      
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            string postResponse = null;
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                postResponse = responseStream.ReadToEnd();
                responseStream.Close();
            }

            return (postResponse);
        }

    }
}
