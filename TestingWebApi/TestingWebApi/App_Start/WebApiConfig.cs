using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using TestingWebApi.Common;

namespace TestingWebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
			config.MapHttpAttributeRoutes();
            #region Web API configuration and services
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            #endregion Web API configuration and services
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            //config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
            //config.MessageHandlers.Add(new LogHandler());
        }
    }


}
