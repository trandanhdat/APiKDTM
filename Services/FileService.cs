
using Microsoft.AspNetCore.Authorization;

namespace APi.Services
{
    public class FileService : IFileService

    {
        private readonly IWebHostEnvironment _enviroment;
        private readonly ILogger<FileService> _logger;
        private readonly string[] _allowedExtensions=  {".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        public FileService(IWebHostEnvironment environment,ILogger<FileService> logger) 
        {
            _enviroment  = environment;
            _logger = logger;
        }
        public async Task<string> UploadImageAsync(IFormFile file, string folder = "products")
        {
            if (!IsValidImageFile(file))
                throw new ArgumentException("File không hợp lệ");


            try
            {
                var yearMonth = DateTime.Now.ToString("MM/yyyy");

                var directory = Path.Combine(_enviroment.WebRootPath,"Uploads",folder,yearMonth);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var unipueFileName = $"{Guid.NewGuid()}-{Path.GetExtension(file.FileName).ToLowerInvariant()}";
                var filePath = Path.Combine(directory, unipueFileName);
                using (var filestream = new FileStream(filePath,FileMode.Create))
                {
                    await file.CopyToAsync(filestream);
                }
                return $"/uploads/{folder}/{yearMonth}/{unipueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload file: {FileName}", file.FileName);
                throw;
            }
        }
        public bool DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                var fullPath = Path.Combine(_enviroment.WebRootPath, imagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Đã xóa file: {ImagePath}", imagePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa file: {ImagePath}", imagePath);
                return false;
            }

        }

        public bool IsValidImageFile(IFormFile file)
        {
            if(file == null || file.Length == 0) {
                return false;
            }
            if (file.Length > MaxFileSize)
            {
                return false;
            }

            var exten = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(exten))
            {
                return false;
            }
            var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;
            return true;
        }


    }
}
