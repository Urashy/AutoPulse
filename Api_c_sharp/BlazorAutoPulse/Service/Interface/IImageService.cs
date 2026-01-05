using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface IImageService: IService<ImageDTO>
{
    public string GetImage(int id);
    public string GetFirstImage(int id);
    public Task<ImageDTO> GetImageProfil(int id);
    Task<List<int>> GetAllImageIdsByVoitureId(int voitureId);

}