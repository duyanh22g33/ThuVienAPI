using WebAPI.Data;
using WebAPI.Models.Domain;

namespace WebAPI.Repositories
{
    public class LocalImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;

        public LocalImageRepository(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public Image Upload(Image image)
        {
            var localFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images", $"{image.FileName}{image.FileExtension}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            image.File.CopyTo(stream);

            var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{image.FileName}{image.FileExtension}";
            image.FilePath = urlFilePath;

            _dbContext.Images.Add(image);
            _dbContext.SaveChanges();

            return image;
        }

        public List<Image> GetAllInfoImages()
        {
            return _dbContext.Images.ToList();
        }

        public (byte[], string, string) DownloadFile(int id)
        {
            var fileById = _dbContext.Images.FirstOrDefault(x => x.Id == id);
            if (fileById == null)
            {
                throw new FileNotFoundException("File not found");
            }

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Images", $"{fileById.FileName}{fileById.FileExtension}");
            var stream = File.ReadAllBytes(path);
            var fileName = fileById.FileName + fileById.FileExtension;

            return (stream, "application/octet-stream", fileName);
        }
    }
}