using Microsoft.AspNetCore.Mvc;
using SecuritySystemsManagerMVC.Models;
using System.Diagnostics;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Home";
            return View();
        }

        public IActionResult Privacy()
        {
            ViewBag.Title = "Privacy Policy";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return RedirectToAction("Error500", "Error");
        }
    }
}
