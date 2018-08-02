using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            ViewBag.CC =  Clyconf.Net.Core.ConfigManager.Instance.GetConfigValue("/1/dbcon");
            return View();
        }
    }
}
