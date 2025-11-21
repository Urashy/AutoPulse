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
        
    }

    public interface ICompteRepository
    {
        Task<IEnumerable<Compte>> GetComptesByTypes(int  type);
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

}
