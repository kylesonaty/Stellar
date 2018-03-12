using System.Linq;

namespace Stellar
{
    public interface ICosmosQueryable<T> : IQueryable<T>
    {

    }

    public interface ICosmosQueryable : IQueryable
    {

    }
}
