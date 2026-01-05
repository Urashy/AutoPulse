using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IPostImageService
    {
        public Task<ImageDTO> CreateAsync(ImageUpload entity);
        public Task UpdateAsync(int id, ImageUpload entity);
    }
}
