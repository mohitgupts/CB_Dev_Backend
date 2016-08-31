using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TestingWebApi.DAL;

namespace TestingWebApi.Common
{
    public class LogHandler : System.Net.Http.DelegatingHandler
    {

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // log request body
                string requestBody = await request.Content.ReadAsStringAsync();
                PaymentHandler pay=new PaymentHandler();
                pay.RequestLog("Request",requestBody);

                // let other handlers process the request
                var result = await base.SendAsync(request, cancellationToken);

                // once response body is ready, log it
                var responseBody = await result.Content.ReadAsStringAsync();
                pay.RequestLog("Response",requestBody);

                return result;
            }
        
    }
}