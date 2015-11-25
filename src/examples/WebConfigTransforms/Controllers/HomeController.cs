using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebConfigTransforms.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.SimpleString = MvcApplication.HoconConfig.GetString("root.simple-string");
            return View();
        }
    }
}
