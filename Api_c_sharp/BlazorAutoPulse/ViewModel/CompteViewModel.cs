using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.ViewModel
{
    public class CompteViewModel
    {
        private readonly ICompteService _compteService;
        private readonly IPostImageService _postImageService;
        private readonly IImageService _imageService;
        private readonly IAnnonceService _annonceService;
        private readonly IAdresseService _addressService;
        public NavigationManager _nav { get; set; }

        public CompteDetailDTO compte;
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

        public bool modalSuppression = false;
        public bool confirmationSuppression = false;
        public string confirmationTexte = "";
        public bool suppressionReussi =  false;
        
        private Action? _refreshUI;

        public CompteViewModel(ICompteService compteService, IPostImageService postImageService, IImageService imageService, IAnnonceService annonceService, IAdresseService adresseService)
        {
            _compteService = compteService;
            _postImageService = postImageService;
            _imageService = imageService;
            _annonceService = annonceService;
            _addressService = adresseService;
        }
        
        public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
        {
            _refreshUI = refreshUI;
            _nav = nav;
            
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
                NumeroSiret = compte.NumeroSiret ?? "",
                RaisonSociale = compte.RaisonSociale ?? "",
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
                imageSource = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
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
            passwordError = null;
            passwordSuccess = null;
            currentPassword = null;
            newPassword = null;
            confirmNewPassword = null;
            currentPasswordValid = true;
            newPasswordValid = true;
            _refreshUI?.Invoke();
        } 
        
    public async Task ChangePassword()
    {
        // Reset des messages
        passwordError = null;
        passwordSuccess = null;
        currentPasswordValid = true;
        newPasswordValid = true;
        
        // Validation côté client basique
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            passwordError = "Le mot de passe actuel est requis";
            currentPasswordValid = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            passwordError = "Le nouveau mot de passe est requis";
            newPasswordValid = false;
            return;
        }

        if (newPassword != confirmNewPassword)
        {
            passwordError = "Les mots de passe ne correspondent pas";
            newPasswordValid = false;
            return;
        }

        isChangingPassword = true;
        _refreshUI?.Invoke();

        try
        {
            // 1. Vérifier le mot de passe actuel
            var verifResult = await _compteService.VerifUser(new ChangementMdp
            {
                Email = compte.Email,
                MotDePasse = currentPassword
            });

            if (!verifResult)
            {
                passwordError = "Le mot de passe actuel est incorrect";
                currentPasswordValid = false;
                return;
            }

            // 2. Changer le mot de passe
            var changementMdp = new ChangementMdp
            {
                IdCompte = compte.IdCompte,
                Email = compte.Email,
                MotDePasse = newPassword
            };

            var result = await _compteService.ChangementMdp(changementMdp);

            if (result.Success)
            {
                passwordSuccess = "Mot de passe modifié avec succès";
                
                // Reset des champs
                currentPassword = null;
                newPassword = null;
                confirmNewPassword = null;

                // Fermer le modal après 2 secondes
                await Task.Delay(2000);
                ClosePasswordModal();
            }
            else
            {
                // ✅ Afficher l'erreur retournée par le backend
                passwordError = result.ErrorMessage;
                newPasswordValid = false;
                
                // Log pour debug
                Console.WriteLine($"Erreur de changement de mot de passe: {result.ErrorMessage}");
                
                if (result.ValidationErrors != null)
                {
                    foreach (var error in result.ValidationErrors)
                    {
                        Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value)}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            passwordError = "Une erreur inattendue s'est produite";
            Console.WriteLine($"Exception ChangePassword: {ex.Message}");
        }
        finally
        {
            isChangingPassword = false;
            _refreshUI?.Invoke();
        }
    }

        public void OpenSuppressionModal()
        {
            modalSuppression = true;
            _refreshUI?.Invoke();
        }

        public void CloseSuppressionModal()
        {
            modalSuppression = false;
            confirmationSuppression = false;
            confirmationTexte = "";
            _refreshUI?.Invoke();
        }

        public void ContinuerSuppression()
        {
            confirmationSuppression = true;
            _refreshUI?.Invoke();
        }

        public void ConfirmDeleteAccount()
        {
            if (compte.Pseudo == confirmationTexte)
            {
                _compteService.Anonymisation(compte.IdCompte);
                suppressionReussi = true;
            }
        }
    }
}