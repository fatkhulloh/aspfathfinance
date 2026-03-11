using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ImsMVC.Helpers
{
    public class LowercaseRouteHandler: MvcRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var url = requestContext.HttpContext.Request.Url?.AbsolutePath;
            if (url != null && url != url.ToLower())
            {
                var qs = requestContext.HttpContext.Request.Url.Query;
                requestContext.HttpContext.Response.RedirectPermanent(url.ToLower() + qs);
                return null;
            }
            return base.GetHttpHandler(requestContext);
        }
    }
}