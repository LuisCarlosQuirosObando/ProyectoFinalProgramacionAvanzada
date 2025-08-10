using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BancoLosPatitos
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_Error()
        {
            Exception ex = Server.GetLastError();

            var httpEx = ex as HttpException;
            Response.Clear();

            var routeData = new RouteData();
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = httpEx != null && httpEx.GetHttpCode() == 404 ? "NotFound" : "Index";

            Server.ClearError();
            Response.TrySkipIisCustomErrors = true;
            IController errControler = new BancoLosPatitos.Controllers.ErrorController();
            errControler.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }
    }
}
