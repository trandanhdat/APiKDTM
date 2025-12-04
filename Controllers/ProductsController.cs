using APi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    //[Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly DbLoaiContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(DbLoaiContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("AllProduct")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _context.products
                    .Include(p => p.Category)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Price,
                        p.ImagePath,
                        p.CreatedAt,
                        p.UpdatedAt,
                        Category = new
                        {
                            p.Category.Id,
                            p.Category.TenLoai
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }

        [HttpGet("GetProduct/{id}")]
        //[Route("v{version:apiVersion}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _context.products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = product,
                    version = "v1.0"

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }
        [HttpGet("GetProduct/{id}")]
        //[Route("v{version:apiVersion}")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetProductV2(int id)
        {
            try
            {
                var product = await _context.products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = product,
                    version = "v2.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }

        [HttpPost("CreateProduct")]
        [Authorize(Roles = "Admin")] // Only Admin can create products
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _context.products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new
                {
                    success = true,
                    message = "Tạo sản phẩm thành công",
                    data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }

        [HttpPut("UpdateProduct/{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can update products
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID không khớp"
                    });
                }

                var existingProduct = await _context.products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm"
                    });
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.ImagePath = product.ImagePath;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật sản phẩm thành công",
                    data = existingProduct
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }

        [HttpDelete("DeleteProduct/{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete products
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm"
                    });
                }

                _context.products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Xóa sản phẩm thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống"
                });
            }
        }
        [HttpGet("Category")]
        public async Task<IActionResult> getCategory()
        {
            var category = _context.loaiModels.ToList();
            if(category == null)
            {
                return BadRequest();
            }
            return Ok(new
            {
                success = true,
                data= category
            });
        }
    }
}
