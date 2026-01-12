using Api_c_sharp.Models.Entity;
using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.IA.Benchmark;

namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface IModeleRepository
    {
        Task<IEnumerable<Modele>> GetModelesByMarqueIdAsync(int marqueId);
    }

    public interface  IAnnonceRepository
    {
        Task<IEnumerable<Annonce>> GetAnnoncesByMiseEnAvant(int miseAvantId, int pageNumber, int pageSize);
        Task<IEnumerable<Annonce>> GetFilteredAnnonces(ParametreRecherche param);
        Task<IEnumerable<Annonce>> GetAnnoncesByCompteFavoris(int compteId);
        Task<IEnumerable<Annonce>> GetAnnoncesByCompteID(int compteId);
        Task<IEnumerable<Annonce>> GetAnnoncesSimilaires(Annonce? annonce);
        Task<bool> EstMasque(int annonceId);

        Task<PaiementDTO> PaiementMiseEnAvant(int idAnnonce);
    }

    public interface ICompteRepository
    {
        Task<IEnumerable<Compte>> GetComptesByTypes(int  type);
        Task<IEnumerable<Compte>> GetCompteByIdAnnonceFavori(int annonceId);
        Task<Compte?> VerifMotDePasse(string email, string hash);
        Task<Compte?> AuthenticateCompte(string email, string hash);
        Task UpdateAnonymise(int idcompte);
        Task UpdateTypeCompte(Compte compteamodif,CompteModifTypeCompteDTO compteModifTypeCompteDTO, bool estpro);
        Task<Compte?> GetProfilPublic(int idcompte);
        Task ToggleEtatCompte(int idcompte, bool estretirer);
        Task EnregistrerA2f(TokenEmail tokenEmail);
        Task ActiverA2f(int idCompte);
        Task DesactiverA2f(int idCompte);
        Task<bool> DoitReactiverA2f(int idCompte);
        Task<(bool A2fActif, DateTime? DerniereActivation)> GetStatutA2f(int idCompte);
    }

    public interface IMessageRepository
    {
        Task<int> GetUnreadMessageCount(int conversationId, int userId);
        Task<IEnumerable<Message>> GetMessagesByConversationAndMarkAsRead(int conversationId, int userId);
    }

    public interface ISignalementRepository
    {
        Task<IEnumerable<Signalement>> GetSignalementsByEtatAndType(int etatId,int typeId,string recherche);
    }

    public interface IAvisRepository
    {
        Task<bool> ExisteDejaAsync(int idCommande, int idJugeur);
        Task<IEnumerable<Avis>> GetAvisByCompteId(int compteId);
    }
        
    public interface ICommandeRepository
    {
        Task<Commande> GetCommandeByConversation(int id);
        Task<IEnumerable<Commande>> GetCommandesByCompteId(int compteId);
    }

    public interface IImageRepository
    {
        Task<IEnumerable<int>> GetAllImagesByVoitureId(int voitureId);
        
        Task<Image?> GetFirstImageByVoitureID(int idvoiture);

        Task<Image?> GetImageByCompteID(int idcompte);
    }
    
    public interface ICouleurRepository
    {
        Task<IEnumerable<Couleur>> GetCouleursByVoitureId(int voitureId);
    }

    public interface IAPourCouleurRepository
    {
        Task<APourCouleur?> GetAPourCouleursByIDS(int voitureId, int couleurId);
    }

    // Ajouter dans IMethodRepository.cs :

    public interface ITokenEmail
    {
        Task<TokenEmail?> VerificationCode(string email, string code, string typeToken);
        Task InvaliderTokensParType(int idCompte, string typeToken);
        Task NettoyerTokensExpires();
    }

    public interface IAdresseRepository
    {
        Task<IEnumerable<Adresse>> GetAdresseByCompteID(int compteId);
    }

    public interface ITypeCompteRepository
    {
        Task<IEnumerable<TypeCompte>> GetTypeComptesPourChercher();
        Task<TypeCompte?> GetTypeCompteByCompteId(int compteID);
    }

    public interface IConversationRepository
    {
        Task<IEnumerable<Conversation>> GetConversationsByCompteID(int compteId);

        Task<Conversation> PostComplet(Conversation conversation,string contenumessage, int idcompte, int idcompte2);
    }

    public interface IApourConversationRepository
    {
        Task<APourConversation?> GetAPourConversationByIDS(int conversationId, int compteId);

        Task<bool> Exists(int idCompte1, int idCompte2,int idannonce);
    }

    public interface IFavoriRepository
    {
        Task<Favori?> GetFavoriByIdsAsync(int idCompte, int idAnnonce);
        Task<bool> ExistsAsync(int idCompte, int idAnnonce);
        Task<IEnumerable<Favori>> GetByCompteIdAsync(int idCompte);
    }

    public interface IVueRepository
    {
        Task<Vue?> GetVueByIdsAsync(int idCompte, int idAnnonce);
    }

    public interface IBloqueRepository
    {
        Task<Bloque?> GetBloqueByIdsAsync(int idCompteBloqueur, int idCompteBloque);
        Task<bool> ExistsAsync(int idCompte, int idAnnonce);
    }

    public interface IPlainteRepository
    {
        Task<IEnumerable<Plainte>> GetPlainteByCompteID(int idCompte);
    }

    public interface IOffreRepository
    {
        Task<IEnumerable<Offre>> GetOffresByMessageIdAsync(int idMessage);
        Task<bool> PendingOfferExistsInConversation(int idConversation);
    }

    public interface  ICarteBancaireRepository
    {
        Task<IEnumerable<CarteBancaire>> GetCarteBancaireByCompteId(int compteid);
    }

    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> StoreRefreshTokenAsync(
            int idCompte,
            string token,
            bool rememberMe,
            string? ipAddress = null,
            string? userAgent = null);

        Task<Compte?> ValidateRefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserTokensAsync(int idCompte);
        Task<int> CleanupExpiredTokensAsync();
        Task<List<RefreshToken>> GetActiveUserTokensAsync(int idCompte);
    }
    public interface IPaiementRepository
    {
        Task<IEnumerable<Paiement>> VerifPaiementAutoMiseEnAvant();
    }
    public interface IFactureRepository
    {
        byte[] GenererPdfFacture(int Commandeid);
    }
}
