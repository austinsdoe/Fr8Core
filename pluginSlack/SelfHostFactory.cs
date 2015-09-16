﻿using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;

namespace pluginSlack
{
    public class SelfHostFactory
    {
        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var config = new HttpConfiguration();

                // Web API routes
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "PluginSlack",
                    routeTemplate: "plugin_slack/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );

                app.UseWebApi(config);
            }
        }

        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostFactory.SelfHostStartup>(url: url);
        }
    }
}
