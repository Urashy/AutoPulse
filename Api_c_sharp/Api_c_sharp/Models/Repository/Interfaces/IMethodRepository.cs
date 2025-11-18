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
    }

}
