using WebAPI.Models.Domain;

namespace WebAPI.Repositories
{
    public interface IImageRepository
    {
        Image Upload(Image image);
        List<Image> GetAllInfoImages();
        (byte[], string, string) DownloadFile(int id);
    }
}