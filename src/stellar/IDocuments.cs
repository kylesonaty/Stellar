namespace Stellar.Documents
{
    public interface IDocuments
    {
        object Store(object entity);
        object Delete(string id);
        T Get<T>(string id);
        ICosmosQueryable<T> Query<T>();
    }
}
