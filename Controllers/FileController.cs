using APi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService,ILogger<FileController> logger,IWebHostEnvironment environment) {
            _fileService = fileService;
            _environment = environment;
            _logger  = logger;
        }

        [HttpPost("/upload")]
        public async Task <IActionResult> UploadFile(IFormFile file)
        {
            if (!_fileService.IsValidImageFile(file))
            {
                _logger.LogInformation("Loi file");
            }

            var path = await _fileService.UploadImageAsync(file, "products");
            return Ok(new {Success = true,path});

        }
        [HttpPost("Delete")]
        public IActionResult DeleteFile(string stringPath)
        {
            var file = _fileService.DeleteImage(stringPath);
            if (!file)
            {
                return NotFound();
            }

            return Ok(new
            {
                Success = true,
                Message = "Xóa file thành công"
            });
        }
        [HttpGet]
        public IActionResult GetFileAll()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(_environment.WebRootPath, "Uploads", "products"));

            FileInfo[] fileInfos =  dir.GetFiles("*.*",SearchOption.AllDirectories);
            var files = new List<object>();
            foreach (var file in fileInfos) {
                files.Add(new
                {
                    FileName = file.Name,
                    Directory = file.FullName,
                    RelativePath = Path.GetRelativePath(_environment.WebRootPath, file.FullName)
                }
                );
            }
            return Ok(files);
        }

    }
}
