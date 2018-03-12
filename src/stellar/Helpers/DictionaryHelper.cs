using System.Collections.Generic;

namespace Stellar
{
    internal static class DictionaryHelper
    {
        internal static IEnumerable<CosmosQueryParameter> ToCosmosQueryParameterList(IDictionary<string, string> dictionary)
        {
            foreach (var item in dictionary)
            {
                yield return new CosmosQueryParameter { Name = item.Key, Value = item.Value };
            }
        }
    }
}