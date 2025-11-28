using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IPostImageService
    {
        public Task<Image> CreateAsync(ImageUpload entity);
        public string GetImage(int id);
        public string GetFirstImage(int id);
        public string GetAllIdImage(int id);
        public string GetImageProfil(int id);
    }
}
