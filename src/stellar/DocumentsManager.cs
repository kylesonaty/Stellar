using Stellar.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Stellar
{
    internal class DocumentsManager : IDocuments
    {
        private readonly string _uri;
        private readonly string _apiKey;
        private readonly string _dbName;
        private readonly string _collectionName;
        private readonly ISerializer _serializer; 

        public DocumentsManager(string uri, string apiKey, string dbName, string collectionName, ISerializer serializer)
        {
            _uri = uri;
            _apiKey = apiKey;
            _collectionName = collectionName;
            _dbName = dbName;
            _serializer = serializer;
        }

        public async Task<CosmosHttpResponse> Delete(string id)
        {
            var queryPath = $"dbs/{_dbName}/colls/{_collectionName}/docs/{id}";
            var response = await GetResourceResult("delete", queryPath, queryPath);
            return response;
        }

        public async Task<T> Get<T>(string id) where T : class
        {
            var queryPath = $"dbs/{_dbName}/colls/{_collectionName}/docs/{id}";
            var response = await GetResourceResult("get", queryPath, queryPath);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return _serializer.Deserailize<T>(response.Body);
        }
        
        public async Task<List<T>> Query<T>(string sql, object param = null)
        {
            var queryPath = $"dbs/{_dbName}/colls/{_collectionName}/docs";
            var resourceValue = queryPath.Substring(0, queryPath.LastIndexOf('/'));
            var regex = new Regex(@"(?<=\bfrom\s)(\w+)");
            var character = regex.Match(sql);
            var result = character.Captures[0];

            sql = sql + " and " + result + "._type = \"" + typeof(T).FullName + "\"";

            var query = CreateCosmosQueryJson(sql, param);
            var response = await GetResourceResult("post", queryPath, resourceValue, query, jsonQuery: true);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new CosmosQueryException(response.Body);


            var cosmosQueryResponse = _serializer.Deserailize<CosmosQueryResponse>(response.Body);
            if (cosmosQueryResponse._count == 0)
                return new List<T>();
            return _serializer.Deserailize<List<T>>(cosmosQueryResponse.Documents.ToString());
        }

        public async Task<CosmosHttpResponse> Store(string id, object entity)
        {
            var queryPath = $"dbs/{_dbName}/colls/{_collectionName}/docs";
            var resourceValue = queryPath.Substring(0, queryPath.LastIndexOf('/'));
            var json = _serializer.Serialize(entity);
            var result = await GetResourceResult("post", queryPath, resourceValue, json, upsert: true);
            return result;
        }

        private async Task<CosmosHttpResponse> GetResourceResult(string verb, string queryPath, string resourceValue = "", string body = "", string resourceType = "docs", bool jsonQuery = false, bool upsert = false)
        {
            try
            {
                var responseMessage = await HttpRequestHelper.ExecuteResourceRequest(verb, _uri, _apiKey, queryPath, "docs", resourceValue, body, jsonQuery, upsert);
                var response = new CosmosHttpResponse
                {
                    StatusCode = responseMessage.StatusCode,
                    Body = await responseMessage.Content.ReadAsStringAsync()
                };
                return response;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        private string CreateCosmosQueryJson(string sql, object param)
        {
            var query = new CosmosQuery { Query = sql, Parameters = new List<CosmosQueryParameter>() };

            if (param != null)
            {
                var dictionary = param.GetType().GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(param)?.ToString() ?? "");

                query.Parameters = DictionaryHelper.ToCosmosQueryParameterList(dictionary);
            }
            
            return _serializer.Serialize(query); // might have to use different method, this one adds type parameter
        }
    }
}