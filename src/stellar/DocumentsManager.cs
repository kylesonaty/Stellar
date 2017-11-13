using Newtonsoft.Json.Linq;
using Stellar.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public async Task<object> Delete(string id)
        {
            var queryPath = $"dbs/{_dbName}/colls/{_collectionName}/docs/{id}";
            var response = await GetResourceResult("delete", queryPath, queryPath);
            return response;
        }

        public async Task<T> Get<T>(string id)
        {
            var queryPath = $"dbs/{_dbName}/colls/{_collectionName}/docs/{id}";
            var json = await GetResourceResult("get", queryPath, queryPath);
            var obj = _serializer.Deserailize<T>(json);
            return obj;
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
            var json = await GetResourceResult("post", queryPath, resourceValue, query, jsonQuery: true);
            var response = _serializer.Deserailize<CosmosQueryResponse>(json);
            if (response._count == 0)
                return new List<T>();
            return _serializer.Deserailize<List<T>>(response.Documents.ToString());
        }

        public async Task<object> Store(string id, object entity)
        {
            var queryPath = $"dbs/{_dbName}/colls/{_collectionName}/docs";
            var resourceValue = queryPath.Substring(0, queryPath.LastIndexOf('/'));
            var json = _serializer.Serialize(entity);
            var result = await GetResourceResult("post", queryPath, resourceValue, json, upsert: true);
            return result;
        }

        private async Task<string> GetResourceResult(string verb, string queryPath, string resourceValue = "", string body = "", string resourceType = "docs", bool jsonQuery = false, bool upsert = false)
        {
            try
            {
                var result = await HttpRequestHelper.ExecuteResourceRequest(verb, _uri, _apiKey, queryPath, "docs", resourceValue, body, jsonQuery, upsert);
                return result;
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

    internal class CosmosQuery
    {
        public string Query { get; set; }
        public IEnumerable<CosmosQueryParameter> Parameters { get; set; }
    }

    internal class CosmosQueryParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

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

    internal class CosmosQueryResponse
    {
        public int _count { get; set; }
        public JArray Documents { get; set; }
    }
}