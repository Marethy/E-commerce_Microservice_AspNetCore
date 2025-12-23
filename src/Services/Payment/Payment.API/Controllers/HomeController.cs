using Microsoft.AspNetCore.Mvc;

namespace Payment.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                service = "Payment.API",
                version = "1.0.0",
                status = "running",
                timestamp = DateTime.UtcNow,
                message = "Payment Service is up and running"
            });
        }
    }
}
