using Stellar.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;

namespace Stellar
{

    internal class CosmosQuery : QueryProvider
    {
        private readonly Uri _uri;
        private readonly string _apiKey;
        private readonly string _basePath;
        private readonly ISerializer _serializer;

        internal CosmosQuery(Uri uri, string apiKey, string basePath, ISerializer serializer)
        {
            _uri = uri;
            _apiKey = apiKey;
            _basePath = basePath;
            _serializer = serializer;
        }

        public override object Execute(Expression expression)
        {
            var resourceValue = _basePath.Substring(0, _basePath.LastIndexOf('/'));
            var command = Translate(expression);
            var query = CreateCosmosQueryJson(command, null);
            var response = HttpRequestHelper.GetResourceResult("post", _uri, _apiKey, _basePath, resourceValue, query, isQuery: true).Result;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new CosmosQueryException(response.Body);

            Type type = TypeSystemHelper.GetElementType(expression.Type);
            var serializationType =  typeof(IEnumerable<>).MakeGenericType(type);
            var cosmosQueryResponse = _serializer.Deserailize<CosmosQueryResponse>(response.Body);
            var docs = _serializer.Deserialize(cosmosQueryResponse.Documents.ToString(), serializationType);
            return docs;
        }

        public override string GetQueryText(Expression expression)
        {
            return Translate(expression);
        }

        private string Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            return new QueryTranslator().Translate(expression);
        }

        private string CreateCosmosQueryJson(string sql, object param)
        {
            var query = new PostCosmosQueryModel { Query = sql, Parameters = new List<CosmosQueryParameter>() };

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
