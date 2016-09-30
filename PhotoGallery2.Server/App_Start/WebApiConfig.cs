using PhotoGalery2.Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;

namespace PhotoGallery2.Server
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // show failure details
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy
                = IncludeErrorDetailPolicy.Always;

            // Web API configuration and services

            config.MessageHandlers.Add(new AuthMessageHandler());

            config.EnableCors();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


            // this line DISABLES XML formatter for convenience of view in Browser
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        }
    }
}
