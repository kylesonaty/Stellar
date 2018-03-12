using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Stellar.Documents
{
    public interface IDocuments
    {
        Task<CosmosHttpResponse> Store(string id, object entity);
        Task<CosmosHttpResponse> Delete(string id);
        Task<T> Get<T>(string id) where T : class;
        Task<List<T>> Query<T>(string sql, object param = null);
        CosmosQueryable<T> Query<T>();
    }
}
