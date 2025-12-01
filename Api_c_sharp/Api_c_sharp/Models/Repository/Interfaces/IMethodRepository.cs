namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface IModeleRepository
    {
        //methode filtre par marque
        Task<IEnumerable<Modele>> GetModelesByMarqueIdAsync(int marqueId);
    }

    public interface  IAnnonceRepository
    {
        Task<IEnumerable<Annonce>> GetAnnoncesByMiseEnAvant(int miseAvantId);
        Task<IEnumerable<Annonce>> GetFilteredAnnonces(int id, int idcarburant, int idmarque, int idmodele, int prixmin, int prixmax, int idtypevoiture, int idtypevendeur, string nom, int kmmin, int kmmax, string departement, int pageNumber, int pageSize);
        Task<IEnumerable<Annonce>> GetAnnoncesByCompteFavoris(int compteId);
    }

    public interface ICompteRepository
    {
        Task<IEnumerable<Compte>> GetComptesByTypes(int  type);
        Task<IEnumerable<Compte>> GetCompteByIdAnnonceFavori(int annonceId);
        Task<Compte> VerifMotDePasse(string email, string hash);
        Task<Compte> AuthenticateCompte(string email, string hash);
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
        Task<IEnumerable<Commande>> GetCommandeByCompteId(int compteId);
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
}
