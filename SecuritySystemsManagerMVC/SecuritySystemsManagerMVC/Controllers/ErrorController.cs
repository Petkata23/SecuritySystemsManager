using Microsoft.AspNetCore.Mvc;

namespace SecuritySystemsManagerMVC.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult Error404()
        {
            Response.StatusCode = 404;
            return View();
        }

        [Route("Error/500")]
        public IActionResult Error500()
        {
            Response.StatusCode = 500;
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            Response.StatusCode = statusCode;
            
            return statusCode switch
            {
                404 => View("Error404"),
                500 => View("Error500"),
                _ => View("Error500") // Default to 500 for unknown errors
            };
        }
    }
} 