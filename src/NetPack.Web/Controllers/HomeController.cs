using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetPack.Pipeline;

namespace NetPack.Web.Controllers
{
    public class HomeController : Controller
    {
        private PipelineManager _pipelineManager;

        public HomeController(PipelineManager pipelineManager)
        {
            _pipelineManager = pipelineManager;
        }
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

        public IActionResult RequireJsOptimised()
        {

            return View();
        }

        public IActionResult RollupBundle()
        {
            return View();
        }

        public IActionResult RollupCodeSplitting()
        {
            return View();
        }

        public IActionResult RequireJsOptimisedTagHelper()
        {

            return View();
        }

        public IActionResult JsMin()
        {

            return View();
        }

        public IActionResult BrowserReload()
        {

            return View();
        }

        public IActionResult SystemHmr()
        {
            return View();
        }

        public IActionResult SystemHmrTagHelper()
        {
            return View();
        }

        public IActionResult RequireJsHmr()
        {
            return View();
        }

        public IActionResult RequireJsHmrTagHelper()
        {
            return View();
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}
