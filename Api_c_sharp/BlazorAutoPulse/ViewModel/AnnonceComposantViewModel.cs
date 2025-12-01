using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel;

public class AnnonceComposantViewModel
{
    private readonly IImageService _imageService;

    public AnnonceComposantViewModel(IImageService imageService)
    {
        _imageService = imageService;
    }
    
    public string GetImage(int id)
    {
        return _imageService.GetImage(id);
    }

    public string GetFirstImage(int id)
    {
        return _imageService.GetFirstImage(id);
    }
}