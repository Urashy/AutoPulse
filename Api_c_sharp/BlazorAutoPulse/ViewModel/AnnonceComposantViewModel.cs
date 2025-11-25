using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel;

public class AnnonceComposantViewModel
{
    private readonly IPostImageService _postImageService;

    public AnnonceComposantViewModel(IPostImageService postImageService)
    {
        _postImageService = postImageService;
    }
    
    public string GetImage(int id)
    {
        return _postImageService.GetImage(id);
    }

    public string GetFirstImage(int id)
    {
        return _postImageService.GetFirstImage(id);
    }
}