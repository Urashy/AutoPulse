using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface IImageService: IService<Image>
{
    public string GetImage(int id);
    public string GetFirstImage(int id);
    public Task<Image> GetImageProfil(int id);
    Task<List<int>> GetAllImageIdsByVoitureId(int voitureId);

}