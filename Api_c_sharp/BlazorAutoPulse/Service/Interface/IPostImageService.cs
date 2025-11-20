using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IPostImageService
    {
        public Task<Image> CreateAsync(ImageUpload entity);
    }
}
