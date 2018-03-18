using System;
using Stellar.Documents;
using Stellar.Serialization;

namespace Stellar
{
    public class CosmosDbAccount
    {
        public Uri ServiceEndpoint { get; }
        public string DatabaseId { get; }
        public string DefaultCollectionId { get; }
        private readonly string _apiKey;
        private readonly ISerializer _serializer;

        /// <summary>
        /// CosmosDbAccount constructor
        /// </summary>
        /// <param name="serviceEndpoint">Endpoint for the CosmosDB service</param>
        /// <param name="authKeyOrResourceToken">API key to use for accessing the CosmosDB service endpoint</param>
        /// <param name="databaseId">The CosmosDB database ID for storing and querying documents</param>
        /// <param name="defaultCollectionId">Default collection for storing and querying documents</param>
        public CosmosDbAccount(string serviceEndpoint, string authKeyOrResourceToken, string databaseId, string defaultCollectionId) :
            this(new Uri(serviceEndpoint), authKeyOrResourceToken, databaseId, defaultCollectionId)
        { }

        /// <summary>
        /// CosmosDbAccount constructor
        /// </summary>
        /// <param name="serviceEndpoint">Endpoint for the CosmosDB service</param>
        /// <param name="authKeyOrResourceToken">API key to use for accessing the CosmosDB service endpoint</param>
        /// <param name="databaseId">The CosmosDB database ID for storing and querying documents</param>
        /// <param name="defaultCollectionId">Default collection for storing and querying documents</param>
        public CosmosDbAccount(Uri serviceEndpoint, string authKeyOrResourceToken, string databaseId, string defaultCollectionId)
        {
            ServiceEndpoint = serviceEndpoint;
            _apiKey = authKeyOrResourceToken;
            DatabaseId = databaseId;
            DefaultCollectionId = defaultCollectionId;
            _serializer = new JsonNetSerializer(); // Not injecting this yet.

            Documents = new DocumentsManager(ServiceEndpoint, _apiKey, DatabaseId, DefaultCollectionId, _serializer);
        }

        public IDocuments Documents { get; }
    }
}