using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        private readonly IDistributedCache _cache;

        public RedisController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetUser()
        {
            var cacheKey = "Users";
            var cacheData= await _cache.GetStringAsync(cacheKey);

            if (cacheData != null) {
                var users = JsonSerializer.Deserialize<List<string>>(cacheData);
                return Ok(new { Source = "Redis Cache", Data = users });
            }
            var data = new List<string>() { "Alice","Bob","Charile"};
            var jsonData = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(cacheKey, jsonData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });

            return Ok(new { Source = "Database", Data = data });
        }
     }
}
