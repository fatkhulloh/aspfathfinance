using ImsMVC.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ImsMVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            string langConstraint = "id|en";
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //Finance
            routes.MapRoute(
                 name: "Finance",
                 url: "{lang}/finance/{action}",
                 defaults: new { controller = "Finance", action = "Index", lang = "id" },
                 constraints: new { lang = langConstraint }
                ).RouteHandler = new LowercaseRouteHandler();

            //Auth
            routes.MapRoute(
              name: "Auth",
              url: "{lang}/auth/{action}",
              defaults: new { controller = "Auth", action = "Index", lang = "id" },
              constraints: new { lang = langConstraint }
             ).RouteHandler = new LowercaseRouteHandler();
            //Home
            routes.MapRoute(
                 name: "LocalizedPage",
                 url: "{lang}/{action}",
                 defaults: new { controller = "Home", action = "Index", lang = "id" },
                 constraints: new { lang = langConstraint }
             ).RouteHandler = new LowercaseRouteHandler();
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
