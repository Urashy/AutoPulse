namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface IModeleRepository
    {
        //methode filtre par marque
        Task<IEnumerable<Modele>> GetModelesByMarqueIdAsync(int marqueId);
    }

}
