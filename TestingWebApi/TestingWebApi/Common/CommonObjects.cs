using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace TestingWebApi.Common
{
    public static class CommonObjects
    {
        public static HttpWebRequest CreateHttpRequestObject(string uri, string method)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add("X-Parse-Application-Id", System.Configuration.ConfigurationManager.AppSettings["ApplicationId"].ToString());
            httpWebRequest.Headers.Add("X-Parse-REST-API-KEY", System.Configuration.ConfigurationManager.AppSettings["RestApiKey"].ToString());
            httpWebRequest.Method = method;
            return httpWebRequest;
        }
        public static HttpWebRequest CreateMasterRequestObject(string uri, string method)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add("X-Parse-Application-Id", System.Configuration.ConfigurationManager.AppSettings["ApplicationId"].ToString());
            httpWebRequest.Headers.Add("X-Parse-Master-Key", System.Configuration.ConfigurationManager.AppSettings["MasterKey"].ToString());
            httpWebRequest.Method = method;
            return httpWebRequest;
        }
    }
}