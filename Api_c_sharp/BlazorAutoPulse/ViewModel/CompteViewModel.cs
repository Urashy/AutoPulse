using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorAutoPulse.ViewModel
{
    public class CompteViewModel
    {
        private readonly ICompteService _compteService;
        private readonly IPostImageService _postImageService;
        private readonly IImageService _imageService;
        public NavigationManager _nav { get; set; }

        public Compte compte;
        private Action? _refreshUI;
        public int idImage;

        public CompteViewModel(ICompteService compteService,  IPostImageService postImageService,  IImageService imageService)
        {
            _compteService = compteService;
            _postImageService = postImageService;
            _imageService = imageService;
        }
        
        public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
        {
            _refreshUI = refreshUI;
            _nav = nav;
            
            compte = new Compte();
            try
            {
                compte = await _compteService.GetMe();
            }
            catch
            {
                _nav.NavigateTo("/connexion");
            }
        }
        
        public async Task UploadImageProfil(InputFileChangeEventArgs e)
        {
            ImageUpload imageProfil = new ImageUpload();
            imageProfil.File = e.File;
            imageProfil.IdCompte = compte.IdCompte;
            Image img = await _postImageService.CreateAsync(imageProfil);
            idImage = img.IdImage;
            
            _refreshUI?.Invoke();
        }

        public async Task ChangeImageProfil(InputFileChangeEventArgs e)
        {
            ImageUpload imageProfil = new ImageUpload();
            imageProfil.File = e.File;
            
            Image img = await _imageService.GetByIdAsync(idImage);
            imageProfil.IdImage = img.IdImage;
            imageProfil.IdCompte = compte.IdCompte;
            imageProfil.IdVoiture = img.IdVoiture;
            
            _postImageService.UpdateAsync(imageProfil.IdImage, imageProfil);
        }
        
        public string GetImageProfil(int id)
        {
            return _imageService.GetImageProfil(id);
        }
    }
}
