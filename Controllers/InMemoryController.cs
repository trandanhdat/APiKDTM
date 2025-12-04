using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using APi.Models;

namespace APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InMemoryController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        private readonly DbLoaiContext _dbLoaiContext;

        public InMemoryController(IMemoryCache cache,DbLoaiContext dbLoaiContext) {
            _cache = cache;
            _dbLoaiContext = dbLoaiContext;
        }
        [HttpGet("products")]
        public IActionResult GetProduct()
        {
            if (!_cache.TryGetValue("products", out List<UserModel>? products))
            {
                Console.WriteLine("🟡 Cache MISS — tạo mới danh sách");
                products = _dbLoaiContext.userModels.ToList();
                //products = new List<string>() { "Laptop", "iPhone", "Samsung" };
                _cache.Set("products", products, TimeSpan.FromMinutes(1));
            }
            else
            {
                Console.WriteLine("🟢 Cache HIT — lấy từ RAM");
            }

            return Ok(new { source = "cache", data = products });
        }

    }
}
