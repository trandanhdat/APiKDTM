namespace APi.Services
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "products");
        bool DeleteImage(string imagePath);
        bool IsValidImageFile(IFormFile file);
    }
}
