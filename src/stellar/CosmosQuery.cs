using System.Collections.Generic;

namespace Stellar
{
    internal class CosmosQuery
    {
        public string Query { get; set; }
        public IEnumerable<CosmosQueryParameter> Parameters { get; set; }
    }
}