using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using static System.Net.WebRequestMethods;

namespace BlazorAutoPulse.ViewModel
{
    public class VenteViewModel
    {
        //-------------------------------- Service
        private readonly IService<Annonce> _annonceService;
        private readonly IService<Image> _imageService;
        private readonly IPostImageService _uploadImageService;

        //-------------------------------- Modele
        public ImageUpload imageUpload;

        private Action? _refreshUI;

        public VenteViewModel(IService<Annonce> annonceService, IService<Image> imageService, IPostImageService uploadImageService)
        {
            _annonceService = annonceService;
            _imageService = imageService;
            _uploadImageService = uploadImageService;
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
            var result = await _uploadImageService.CreateAsync(imageUpload);
            Console.WriteLine(result);
            _refreshUI?.Invoke();
        }
    }
}