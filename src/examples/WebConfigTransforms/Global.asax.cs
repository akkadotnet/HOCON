using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebConfigTransforms
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static Config HoconConfig;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            HoconConfig = ConfigurationFactory.Load();
        }
    }
}
