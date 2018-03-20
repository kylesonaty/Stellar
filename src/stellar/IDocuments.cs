using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stellar.Documents
{
    public interface IDocuments
    {
        Task<CosmosHttpResponse> Store(string id, object entity);
        Task<CosmosHttpResponse> Delete(string id);
        Task<T> Get<T>(string id) where T : class;
        Task<IEnumerable<T>> Query<T>(string sql, object param = null);
        Task<IEnumerable<T2>> Query<T1, T2>(string sql, object param = null);
        CosmosQueryable<T> Query<T>();
    }
}