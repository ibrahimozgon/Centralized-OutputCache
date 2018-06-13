using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace OutputCacheExample.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            var val = DisplayMode(new HttpContextWrapper(context)) == "Mobile";
            if (custom.ToLowerInvariant() == "ismobile")
                return val ? "mobile" : "web";

            return base.GetVaryByCustomString(context, custom);
        }
        public static string DisplayMode(HttpContextBase httpContext)
        {
            var mode = DisplayModeProvider.Instance.Modes.FirstOrDefault(t => t.CanHandleContext(httpContext));
            return mode == null ? string.Empty : mode.DisplayModeId;
        }
    }
}
