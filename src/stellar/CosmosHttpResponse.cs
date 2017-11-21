using System.Net;

namespace Stellar
{
    public class CosmosHttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Body { get; set; }
    }
}