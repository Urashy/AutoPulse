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
        
        public bool showPasswordModal = false;
        public string currentPassword = "";
        public string newPassword = "";
        public string confirmNewPassword = "";
        public string passwordError = "";
        public string passwordSuccess = "";
        public bool isChangingPassword = false;
        public bool currentPasswordValid = true;
        public bool newPasswordValid = true;
        
        private Action? _refreshUI;

        public CompteViewModel(ICompteService compteService, IPostImageService postImageService, IImageService imageService)
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
            if (idImage == 0)
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
            try
            {
                Image? img = await _imageService.GetImageProfil(id);

                imageSource = "";
                if (img != null && img.Fichier != null && img.Fichier.Length > 0)
                {
                    idImage = img.IdImage;
                    var base64 = Convert.ToBase64String(img.Fichier);
                    imageSource = $"{mimeType}{base64}";
                }
                else
                {
                    imageSource = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
                }

                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'affichage de l'image : {ex.Message}");
                imageSource = "images/default-profile.png";
                _refreshUI?.Invoke();
            }
        }

        public void SetActiveSection(string section)
        {
            activeSection = section;
        }
        
        public void ToggleEdit()
        {
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

        public void OpenPasswordModal()
        {
            showPasswordModal = true;
            currentPassword = "";
            newPassword = "";
            confirmNewPassword = "";
            passwordError = "";
            passwordSuccess = "";
            currentPasswordValid = true;
            newPasswordValid = true;
            _refreshUI?.Invoke();
        }

        public void ClosePasswordModal()
        {
            showPasswordModal = false;
            currentPassword = "";
            newPassword = "";
            confirmNewPassword = "";
            passwordError = "";
            passwordSuccess = "";
            _refreshUI?.Invoke();
        }

        public async Task ChangePassword()
        {
            passwordError = "";
            passwordSuccess = "";
            currentPasswordValid = true;
            newPasswordValid = true;

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                passwordError = "Veuillez entrer votre mot de passe actuel.";
                currentPasswordValid = false;
                _refreshUI?.Invoke();
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                passwordError = "Veuillez entrer un nouveau mot de passe.";
                newPasswordValid = false;
                _refreshUI?.Invoke();
                return;
            }

            if (string.IsNullOrWhiteSpace(confirmNewPassword))
            {
                passwordError = "Veuillez confirmer votre nouveau mot de passe.";
                newPasswordValid = false;
                _refreshUI?.Invoke();
                return;
            }

            if (newPassword != confirmNewPassword)
            {
                passwordError = "Les nouveaux mots de passe ne correspondent pas.";
                newPasswordValid = false;
                _refreshUI?.Invoke();
                return;
            }

            if (newPassword.Length < 8)
            {
                passwordError = "Le nouveau mot de passe doit contenir au moins 8 caractères.";
                newPasswordValid = false;
                _refreshUI?.Invoke();
                return;
            }

            isChangingPassword = true;
            _refreshUI?.Invoke();

            try
            {
                ChangementMdp verif = new ChangementMdp
                {
                    Email = compte.Email,
                    MotDePasse = newPassword
                };
                bool mdpCorrect = await _compteService.VerifUser(verif);

                if (!mdpCorrect)
                {
                    passwordError = "Le mot de passe actuel est incorrect.";
                    currentPasswordValid = false;
                    isChangingPassword = false;
                    _refreshUI?.Invoke();
                    return;
                }

                ChangementMdp nouveauMdp = new ChangementMdp
                {
                    IdCompte = compte.IdCompte,
                    MotDePasse = newPassword
                };

                _compteService.ChangementMdp(nouveauMdp);

                passwordSuccess = "Mot de passe modifié avec succès !";
                currentPasswordValid = true;
                _refreshUI?.Invoke();

                await Task.Delay(1000);
                ClosePasswordModal();
            }
            catch (Exception ex)
            {
                passwordError = "Une erreur est survenue lors du changement du mot de passe.";
                currentPasswordValid = false;
                isChangingPassword = false;
                _refreshUI?.Invoke();

                Console.WriteLine($"Erreur lors du changement de mot de passe: {ex.Message}");
            }
        }
    }
}