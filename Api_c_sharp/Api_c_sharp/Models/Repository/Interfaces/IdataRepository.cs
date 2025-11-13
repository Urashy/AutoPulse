namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface IdataRepository<T, TKey> : ReadableRepository<T>, WritableRepository<T>, SearchableRepository<T, TKey> where T : class
    {

    }
}
