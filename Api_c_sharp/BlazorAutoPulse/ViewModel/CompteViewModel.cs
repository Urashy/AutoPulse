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
        public NavigationManager _nav { get; set; }

        public Compte compte;
        private Action? _refreshUI;

        public CompteViewModel(ICompteService compteService,  IPostImageService postImageService)
        {
            _compteService = compteService;
            _postImageService = postImageService;
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
            await _postImageService.CreateAsync(imageProfil);
            
            _refreshUI?.Invoke();
        }
        
        public string GetImageProfil(int id)
        {
            return _postImageService.GetImageProfil(id);
        }
    }
}
