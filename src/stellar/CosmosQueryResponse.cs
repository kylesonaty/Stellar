using Newtonsoft.Json.Linq;

namespace Stellar
{
    internal class CosmosQueryResponse
    {
        public int _count { get; set; }
        public JArray Documents { get; set; }
    }
}