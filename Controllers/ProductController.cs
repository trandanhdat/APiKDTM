//using APi.Models;
//using APi.Models.DTO;
//using APi.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Numerics;

//namespace APi.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ProductController : ControllerBase
//    {
//        private readonly IProductService _productService;
//        private readonly ILogger<ProductService> _logger;
//        private readonly DbLoaiContext _dbContext;

//        public ProductController(IProductService productService,ILogger<ProductService> logger,
//            DbLoaiContext dbLoaiContext) {
//            _productService = productService;
//            _logger = logger;
//            _dbContext = dbLoaiContext;
//        }
//        [HttpPost("CreateProductWithImage")]
//        [Consumes("multipart/form-data")]
//        [Authorize]
//        public async Task<IActionResult> CreateProductWithImage([FromForm] ProductUploadDto dto)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                    return BadRequest(ModelState);

//                var result = await _productService.CreateProductAsync(dto);
//                return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi khi tạo sản phẩm");
//                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý");
//            }

//        }
//        [HttpPost("{id}/UploadProductImage")]
//        [Consumes("multipart/form-data")]
//        public async Task<IActionResult> UploadProductImage(int id, [FromForm] IFormFile image)
//        {
//            try
//            {
//                if (image == null)
//                    return BadRequest("Vui lòng chọn file ảnh");

//                var result = await _productService.UpdateProductImageAsync(id, image);
//                if (result == null)
//                    return NotFound("Không tìm thấy sản phẩm");

//                return Ok(result);
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi khi upload ảnh cho sản phẩm {ProductId}", id);
//                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý");
//            }
//        }

//        [HttpGet("{id}/GetProduct")]
//        public async Task<IActionResult> GetProduct(int id)
//        {
//            var product = await _productService.GetProductByIdAsync(id);
//            if (product == null)
//                return NotFound();

//            return Ok(product);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
//        {
//            var products = await _productService.GetAllProductsAsync();

//            // Phân trang
//            var totalItems = products.Count;
//            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
//            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

//            var response = new
//            {
//                Data = pagedProducts,
//                Page = page,
//                PageSize = pageSize,
//                TotalItems = totalItems,
//                TotalPages = totalPages
//            };

//            return Ok(response);
//        }

//        [HttpPut("{id}/UpdateProduct")]
//        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUploadDto dto)
//        {
//            try
//            {
//                if (!ModelState.IsValid)
//                    return BadRequest(ModelState);

//                var existingProduct = await _productService.GetProductByIdAsync(id);
//                if (existingProduct == null)
//                    return NotFound();

//                var result = await _productService.UpdateProductAsync(id, dto);
//                return Ok(result);
//            }
//            catch (ArgumentException ex)
//            {
//                return BadRequest(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm {ProductId}", id);
//                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý");
//            }
//        }

//        [HttpDelete("{id}/DeleteProduct")]
//        public async Task<IActionResult> DeleteProduct(int id)
//        {
//            try
//            {
//                var result = await _productService.DeleteProductAsync(id);
//                if (!result)
//                    return NotFound();

//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Lỗi khi xóa sản phẩm {ProductId}", id);
//                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý");
//            }
//        }
//        [HttpGet("seed")]
//        public IActionResult SeedAdmin()
//        {
//            var user = new UserModel
//            {
//                username = "admin",
//                password = "admin",
//                email="dat@gmail.com",
//                phone = "0199232323"
//            };
//            _dbContext.Add(user);
//            _dbContext.SaveChanges();
//            return Ok();
//        }

//    }
//}
