using APi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoaiController : ControllerBase
    {

        public List<LoaiModel> loaiModels { get; set; }
        [HttpGet]
        public IActionResult GetAll()
        {
            loaiModels = new List<LoaiModel>();
  

            return Ok(loaiModels);
        }
    }
}
