using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspNetWebApp.Models;
using Hocon;

namespace AspNetWebApp.Controllers
{
    public class HomeController : Controller
    {
        private string _hocon;

        public HomeController()
        {
            _hocon = System.IO.File.ReadAllText("appsettings.conf");
        }

        public IActionResult Index()
        {
            ViewData["HOCON"] = _hocon;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
