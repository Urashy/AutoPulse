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
        Task<IEnumerable<Annonce>> GetFilteredAnnonces(int id, int idcarburant, int idmarque, int idmodele, int prixmin, int prixmax, int idtypevoiture, int idtypevendeur, string nom, int kmmin, int kmmax, string departement);
        Task<IEnumerable<Annonce>> GetAnnoncesByCompteFavoris(int compteId);
    }

    public interface ICompteRepository
    {
        Task<IEnumerable<Compte>> GetComptesByTypes(int  type);
        Task<IEnumerable<Compte>> GetCompteByIdAnnonceFavori(int annonceId);
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
        Task<IEnumerable<Image>> GetImagesByVoitureId(int voitureId);
    }
    
    public interface ICouleurRepository
    {
        Task<IEnumerable<Couleur>> GetCouleursByVoitureId(int voitureId);
    }

}
