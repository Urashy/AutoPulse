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
        public Compte compteEdit;
        
        private string mimeType = "data:image/jpeg;base64,";
        public string imageSource;
        public int idImage;

        public string activeSection = "annonces";
        public bool isEditing = false;
        
        private Action? _refreshUI;

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
            
            await GetImageProfil(compte.IdCompte);
            
            compteEdit = new Compte
            {
                IdCompte = compte.IdCompte,
                Pseudo = compte.Pseudo,
                Nom = compte.Nom,
                Prenom = compte.Prenom,
                Email = compte.Email,
                DateNaissance = compte.DateNaissance,
                Biographie = compte.Biographie,
                IdTypeCompte = compte.IdTypeCompte,
                NumeroSiret = compte.NumeroSiret,
                RaisonSociale = compte.RaisonSociale,
                IdImage = idImage,
            };
        }

        public async Task UpdateProfileImage(InputFileChangeEventArgs e)
        {
            if (imageSource == null)
            {
                await UploadImageProfil(e);
            }
            else
            {
                await ChangeImageProfil(e);
            }
        }
        
        private async Task UploadImageProfil(InputFileChangeEventArgs e)
        {
            ImageUpload imageProfil = new ImageUpload();
            imageProfil.File = e.File;
            imageProfil.IdCompte = compte.IdCompte;
            Image img = await _postImageService.CreateAsync(imageProfil);
            
            await GetImageProfil(compte.IdCompte);
        }

        private async Task ChangeImageProfil(InputFileChangeEventArgs e)
        {
            Console.WriteLine(e.File.Name);
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

        public void SetActiveSection(string section)
        {
            activeSection = section;
        }
        
        public void ToggleEdit()
        {;
            isEditing = true;
            _refreshUI?.Invoke();
        }

        public async Task SaveProfile()
        {
            await _compteService.UpdateAsync(compte.IdCompte, compteEdit);
            compte = await _compteService.GetMe();
            _refreshUI?.Invoke();
            isEditing = false;
        }

        public void CancelEdit()
        {
            isEditing = false;
        }
    }
}
