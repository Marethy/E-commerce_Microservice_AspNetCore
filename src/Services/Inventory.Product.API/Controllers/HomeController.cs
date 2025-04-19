using Microsoft.AspNetCore.Mvc;

namespace Inventory.Product.API.Controllers
{
    public class HomeController:ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return Redirect("/swagger");
        }
        [HttpGet("error")]
        public IActionResult Error()
        {
            return Problem();
        }
    }
}
