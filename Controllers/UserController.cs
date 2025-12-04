using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using APi.Models;

namespace APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DbLoaiContext _dbLoaiContext;

        public UserController(DbLoaiContext dbLoaiContext)
        {
            _dbLoaiContext  = dbLoaiContext;
        }
        [HttpGet("User")]
        public IActionResult AllUser()
        {
            var users = _dbLoaiContext.userModels.ToList();
            return Ok(users);
        }
    }
}
