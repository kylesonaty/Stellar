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
            var translateResult = Translate(expression);
            var projector = translateResult.Projector.Compile();
            var query = CreateCosmosQueryJson(translateResult.CommandText, null);
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
            return Translate(expression).CommandText;
        }

        private TranslateResult Translate(Expression expression)
        {
            var projection = expression as ProjectionExpression;
            if (projection == null)
            {
                expression = Evaluator.PartialEval(expression);
                expression = new QueryBinder().Bind(expression);
                expression = new RedundantSubqueryRemover().Remove(expression);
                projection = (ProjectionExpression)expression;
            }
            
            var commandText = new QueryFormatter().Format(projection.Source);
            var projector = new ProjectionBuilder().Build(projection.Projector);
            return new TranslateResult { CommandText = commandText, Projector = projector };
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
