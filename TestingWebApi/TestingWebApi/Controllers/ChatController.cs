using PusherServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TestingWebApi.Models;

namespace TestingWebApi.Controllers
{
    [RoutePrefix("gage")]
    public class ChatController : ApiController
    {
        private IPusher _pusher;
        public ChatController()
        {
            string applicationKey = ConfigurationManager.AppSettings["Pusher_Application_Key"];
            string applicaitonSecret = ConfigurationManager.AppSettings["Pusher_Application_Secret"];
            string applicationId = ConfigurationManager.AppSettings["Pusher_Application_Id"];
            _pusher = new Pusher(applicationId, applicationKey, applicaitonSecret);
        }

        [HttpPost]
        [Route("InstaPushChat")]
        public HttpResponseMessage Post(PusherMessage message)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Accepted);
            _pusher.Trigger(message.channel, message.eventPush, message);

            string msg = "Successfull";
            response = Request.CreateResponse<string>(HttpStatusCode.OK, msg);
            return response;
        }
        [HttpPost]
        [Route("PushChat")]
        public HttpResponseMessage Post(ChatMessage message)
        {
            string msg = string.Empty;
            PushMessage pushMsg;
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Accepted);
            if (message != null)
            {
                try
                {
                    pushMsg = GetPushMessageObject(message);
                    _pusher.Trigger(pushMsg.channel, pushMsg.eventPush, pushMsg);
                    msg = "Successful";
                }
                catch (Exception ex)
                {
                    msg = "Some error has occured";
                }

            }
            else
            {
                msg = "Unsuccessful";
            }
            response = Request.CreateResponse<string>(HttpStatusCode.OK, msg);
            return response;
        }
        private PushMessage GetPushMessageObject(ChatMessage message)
        {
            PushMessage pushMsg = new PushMessage();
            pushMsg.message = message.message;
            pushMsg.messagea_id = message.messagea_id;
            pushMsg.channel = message.channel;
            pushMsg.eventPush = message.eventPush;
            if (message.userId != null)
            {
                ParsePointer temp = new ParsePointer();
                temp.__type = "Pointer";
                temp.className = "_User";
                temp.objectId = message.userId;
                pushMsg.user = temp;
            }
            return pushMsg;
        }
        [HttpPost]
        [Route("DummyPost")]
        public HttpResponseMessage Post(DummyPointer eventPush)
        {
            HttpResponseMessage response;
            //_pusher.Trigger(channel, eventPush, message);

            response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}