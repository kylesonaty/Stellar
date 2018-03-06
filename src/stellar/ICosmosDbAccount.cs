using Stellar.Documents;
using Stellar.Serialization;

namespace Stellar
{
    public class CosmosDbAccount
    {
        private readonly string _uri;
        private readonly string _apiKey;
        private readonly string _dbName;
        private readonly string _collectionName;
        private readonly ISerializer _serializer;

        public CosmosDbAccount(string uri, string apiKey, string dbName, string collectionName)
        {
            _uri = uri;
            _apiKey = apiKey;
            _dbName = dbName;
            _collectionName = collectionName;
            _serializer = new JsonNetSerializer(); // Not injecting this yet.

            Documents = new DocumentsManager(_uri, _apiKey, _dbName, _collectionName, _serializer);
        }

        public IDocuments Documents { get; }
    }
}
