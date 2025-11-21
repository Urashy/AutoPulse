using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using static System.Net.WebRequestMethods;

namespace BlazorAutoPulse.ViewModel
{
    public class VenteViewModel
    {
        //-------------------------------- Service
        private readonly IAnnonceService _annonceService;
        private readonly IService<Image> _imageService;
        private readonly IPostImageService _postImageService;

        //-------------------------------- Modele
        public ImageUpload imageUpload;

        private Action? _refreshUI;

        public VenteViewModel(IAnnonceService annonceService, IService<Image> imageService, IPostImageService postImageService)
        {
            _annonceService = annonceService;
            _imageService = imageService;
            _postImageService = postImageService;
            imageUpload = new ImageUpload();
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
        }

        public async Task UploadImage(InputFileChangeEventArgs e)
        {
            imageUpload.File = e.File;

            _refreshUI?.Invoke();
        }

        public async Task CreateAnnonce()
        {
            var result = await _postImageService.CreateAsync(imageUpload);
            Console.WriteLine(result);
            _refreshUI?.Invoke();
        }
    }
}