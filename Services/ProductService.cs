using APi.Models;
using APi.Models.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace APi.Services
{
    public class ProductService : IProductService
    {
        private readonly DbLoaiContext _dbLoaiContext;
        private readonly ILogger<ProductService> _logger;
        private readonly IFileService _fileService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(DbLoaiContext dbLoaiContext
            , ILogger<ProductService> logger
            ,IFileService fileService
            ,IHttpContextAccessor httpContextAccessor)
        {
            _dbLoaiContext = dbLoaiContext;
            _logger = logger;
            _fileService = fileService;
            _httpContextAccessor= httpContextAccessor;
        }
        public async Task<ProductResponseDto> CreateProductAsync(ProductUploadDto dto)
        {
            using var transaction = await _dbLoaiContext.Database.BeginTransactionAsync();
            try
            {
                if (!_fileService.IsValidImageFile(dto.Image))
                    throw new ArgumentException("File không hợp lệ");

                var imagePath = await _fileService.UploadImageAsync(dto.Image);

                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    ImagePath = imagePath,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbLoaiContext.products.Add(product);
                await _dbLoaiContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Đã tạo sản phẩm mới với ID: {ProductId}", product.Id);
                return MapToResponseDto(product);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo sản phẩm: {ProductName}", dto.Name);
                throw;
            }
        }

        public async Task<ProductResponseDto> UpdateProductImageAsync(int id, IFormFile image)
        {
            using var transaction = await _dbLoaiContext.Database.BeginTransactionAsync();
            try
            {
                var product = await _dbLoaiContext.products.FindAsync(id);
                if (product == null)
                    return null;

                if (!_fileService.IsValidImageFile(image))
                    throw new ArgumentException("File không hợp lệ");

                var oldImagePath = product.ImagePath;
                var newImagePath = await _fileService.UploadImageAsync(image);

                product.ImagePath = newImagePath;
                product.UpdatedAt = DateTime.UtcNow;

                await _dbLoaiContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Xóa ảnh cũ
                if (!string.IsNullOrEmpty(oldImagePath))
                    _fileService.DeleteImage(oldImagePath);

                _logger.LogInformation("Đã cập nhật ảnh cho sản phẩm ID: {ProductId}", id);
                return MapToResponseDto(product);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi cập nhật ảnh sản phẩm ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductUploadDto dto)
        {
            using var transaction = await _dbLoaiContext.Database.BeginTransactionAsync();
            try
            {
                var product = await _dbLoaiContext.products.FindAsync(id);
                if (product == null)
                    throw new ArgumentException("Không tìm thấy sản phẩm");

                // Cập nhật thông tin cơ bản
                product.Name = dto.Name;
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.UpdatedAt = DateTime.UtcNow;

                // Cập nhật ảnh nếu có
                if (dto.Image != null)
                {
                    if (!_fileService.IsValidImageFile(dto.Image))
                        throw new ArgumentException("File không hợp lệ");

                    var oldImagePath = product.ImagePath;
                    var newImagePath = await _fileService.UploadImageAsync(dto.Image);
                    product.ImagePath = newImagePath;

                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(oldImagePath))
                        _fileService.DeleteImage(oldImagePath);
                }

                await _dbLoaiContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Đã cập nhật sản phẩm ID: {ProductId}", id);
                return MapToResponseDto(product);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm ID: {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductResponseDto> GetProductByIdAsync(int id)
        {
            var product = await _dbLoaiContext.products.FindAsync(id);
            return product == null ? null! : MapToResponseDto(product);
        }

        public async Task<List<ProductResponseDto>> GetAllProductsAsync()
        {
            var products = await _dbLoaiContext.products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(MapToResponseDto).ToList();
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            using var transaction = await _dbLoaiContext.Database.BeginTransactionAsync();
            try
            {
                var product = await _dbLoaiContext.products.FindAsync(id);
                if (product == null)
                    return false;

                var imagePath = product.ImagePath;

                _dbLoaiContext.products.Remove(product);
                await _dbLoaiContext.SaveChangesAsync();
                await transaction.CommitAsync();

                if (!string.IsNullOrEmpty(imagePath))
                    _fileService.DeleteImage(imagePath);

                _logger.LogInformation("Đã xóa sản phẩm ID: {ProductId}", id);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm ID: {ProductId}", id);
                throw;
            }
        }

        private ProductResponseDto MapToResponseDto(Product product)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = !string.IsNullOrEmpty(product.ImagePath)
                    ? $"{baseUrl}{product.ImagePath}"
                    : null!,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
               
            };
        }
    }
}
