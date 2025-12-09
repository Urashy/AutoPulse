using Api_c_sharp.Models.Entity;
using AutoPulse.Shared.DTO;

namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface IModeleRepository
    {
        Task<IEnumerable<Modele>> GetModelesByMarqueIdAsync(int marqueId);
    }

    public interface  IAnnonceRepository
    {
        Task<IEnumerable<Annonce>> GetAnnoncesByMiseEnAvant(int miseAvantId);
        Task<IEnumerable<Annonce>> GetFilteredAnnonces(ParametreRecherche param, int pageNumber, int pageSize, int orderbyprix);
        Task<IEnumerable<Annonce>> GetAnnoncesByCompteFavoris(int compteId);
        Task<IEnumerable<Annonce>> GetAnnoncesByCompteID(int compteId);
    }

    public interface ICompteRepository
    {
        Task<IEnumerable<Compte>> GetComptesByTypes(int  type);
        Task<IEnumerable<Compte>> GetCompteByIdAnnonceFavori(int annonceId);
        Task<Compte> VerifMotDePasse(string email, string hash);
        Task<Compte> AuthenticateCompte(string email, string hash);
        Task UpdateAnonymise(int idcompte);
    }

    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetMessagesByConversation(int conversationId);
    }

    public interface ISignalementRepository
    {
        Task<IEnumerable<Signalement>> GetSignalementsByEtat(int etatId);
    }

    public interface IJournalRepository
    {
        Task<IEnumerable<Journal>> GetJournalByType(int typeID);
    }

    public interface IAvisRepository
    {
        Task<IEnumerable<Avis>> GetAvisByCompteId(int compteId);
    }
        
    public interface ICommandeRepository
    {
        Task<IEnumerable<Commande>> GetCommandesByCompteId(int compteId);
    }

    public interface IImageRepository
    {
        Task<IEnumerable<int>> GetAllImagesByVoitureId(int voitureId);
        
        Task<Image> GetFirstImageByVoitureID(int idvoiture);

        Task<Image> GetImageByCompteID(int idcompte);
    }
    
    public interface ICouleurRepository
    {
        Task<IEnumerable<Couleur>> GetCouleursByVoitureId(int voitureId);
    }

    public interface IAPourCouleurRepository
    {
        Task<APourCouleur> GetAPourCouleursByIDS(int voitureId, int couleurId);
    }

    public interface IReinitialisationMotDePasse
    {
        Task<ReinitialisationMotDePasse> VerificationCode(string email, string code);
    }

    public interface IAdresseRepository
    {
        Task<IEnumerable<Adresse>> GetAdresseByCompteID(int compteId);
    }

    public interface ITypeCompteRepository
    {
        Task<IEnumerable<TypeCompte>> GetTypeComptesPourChercher();
    }

    public interface IConversationRepository
    {
        Task<IEnumerable<Conversation>> GetConversationsByCompteID(int compteId);
    }
}
