using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using LaidigSystemsC.Models;
using System.Data.Entity;
using System.Web.Caching;
using System.Net;

namespace LaidigSystemsC
{
    public class MvcApplication : System.Web.HttpApplication
    {
      
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            string Timeschedule = System.IO.File.ReadLines(HttpContext.Current.Server.MapPath("~/Settings.ini")).First();
            string Location = System.IO.File.ReadLines(HttpContext.Current.Server.MapPath("~/Settings.ini")).Skip(1).Take(1).First();
            string[] timearray = Timeschedule.Split('#');
            string[] locationArray = Location.Split('#');

            string delayedScheduleTime = timearray[1].ToString();
            string filePath = locationArray[1].ToString();
            Application["ScheduleTime"] = delayedScheduleTime;
            Application["DelayedUploadLocation"] = filePath;
            JobScheduler.Start(delayedScheduleTime);
           

        }
      
    }
}
