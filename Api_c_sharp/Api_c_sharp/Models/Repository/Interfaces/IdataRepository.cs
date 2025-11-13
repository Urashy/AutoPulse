namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface IdataRepository<T> : ReadableRepository<T>, WritableRepository<T>, SearchableRepository<T> where T : class
    {

    }
}
