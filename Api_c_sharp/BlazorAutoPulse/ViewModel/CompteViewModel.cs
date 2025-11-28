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
        private string mimeType = "data:image/jpeg;base64,";
        public string imageSource;
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
                await GetImageProfil(compte.IdCompte);
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
            
            await GetImageProfil(compte.IdCompte);
        }

        public async Task ChangeImageProfil(InputFileChangeEventArgs e)
        {
            ImageUpload imageProfil = new ImageUpload();
            imageProfil.File = e.File;
            
            imageProfil.IdImage = idImage;
            imageProfil.IdCompte = compte.IdCompte;
            imageProfil.IdVoiture = null;
            
            await _postImageService.UpdateAsync(imageProfil.IdImage, imageProfil);
            await GetImageProfil(compte.IdCompte);
        }
        
        private async Task GetImageProfil(int id)
        {
            Image img = await _imageService.GetImageProfil(id);
            imageSource = "";
            if (img != null && img.Fichier != null && img.Fichier.Length > 0)
            {
                idImage = img.IdImage;
                var base64 = Convert.ToBase64String(img.Fichier);
                imageSource = $"{mimeType}{base64}";
                _refreshUI?.Invoke();
            }
        }
    }
}
