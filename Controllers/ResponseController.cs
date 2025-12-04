using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponseController : ControllerBase
    {
        [HttpGet("time")]
        [ResponseCache(Duration =10)]
        public IActionResult GetTime()
        {
            return Ok(new
            {
                Message = "Current server time",
                Time = DateTime.Now.ToString("HH:mm:ss")
            });

        }
    }
}
