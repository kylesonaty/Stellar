using System.Collections.Generic;

namespace Stellar
{
    internal class PostCosmosQueryModel
    {
        public string Query { get; set; }
        public IEnumerable<CosmosQueryParameter> Parameters { get; set; }
    }
}