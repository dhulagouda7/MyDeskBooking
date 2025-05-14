using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MyDeskBooking
{    public class MvcApplication : System.Web.HttpApplication
    {        
        protected void Application_Start()
        {            
            AreaRegistration.RegisterAllAreas();
            UnityConfig.RegisterComponents(); // Register Unity container
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Initialize and register bundles
            BundleTable.EnableOptimizations = true;
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            MyDeskBooking.DataAccess.DatabaseInitializer.Initialize();
        }
    }
}
