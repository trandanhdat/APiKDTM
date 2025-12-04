using APi.Models.DTO;

namespace APi.Services
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateProductAsync(ProductUploadDto dto);
        Task<ProductResponseDto> UpdateProductImageAsync(int id, IFormFile image);
        Task<ProductResponseDto> UpdateProductAsync(int id, ProductUploadDto dto);
        Task<ProductResponseDto> GetProductByIdAsync(int id);
        Task<List<ProductResponseDto>> GetAllProductsAsync();
        Task<bool> DeleteProductAsync(int id);
    }
}
