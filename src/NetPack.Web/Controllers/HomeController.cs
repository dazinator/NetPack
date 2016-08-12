using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NetPack.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SingleTypescriptFile()
        {
           // ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult MultipleTypescriptFiles()
        {

            return View();
        }

        public IActionResult MultipleTypescriptFilesCombinedToSingleJsFile()
        {

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
