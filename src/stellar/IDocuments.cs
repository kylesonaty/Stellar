using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stellar.Documents
{
    public interface IDocuments
    {
        Task<object> Store(string id, object entity);
        Task<object> Delete(string id);
        Task<T> Get<T>(string id);
        Task<List<T>> Query<T>(string sql, object param = null);
    }
}
