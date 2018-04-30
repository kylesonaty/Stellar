using System;
using System.Linq;
using Tests;
using Xunit;

namespace Stellar.Tests
{
    public class SimpleQueryWithWhere
    {
        private CosmosDbAccount BogusCosmosDbAccount => new CosmosDbAccount(@"http://www.bogusendpoint.com", "bogustoken", "bogusdb", "");

        private const int testValue = 20;

        [Fact]
        public void WhereClauseShouldIncludeTestCondition()
        {
            var testObjectsQueryAsString = BogusCosmosDbAccount.Documents.Query<TestObject>()
                                                .Where(x => x.SomeIntProperty == 20)
                                                .ToString().ToLower();

            Assert.Contains("where", testObjectsQueryAsString);
            Assert.Contains("someintproperty", testObjectsQueryAsString);
            Assert.Contains("20", testObjectsQueryAsString);
        }

        [Fact]
        public void WhereClauseShouldIncludeTestValueWhetherLiteralOrFromAVariable()
        {
            var testObjectsQueryAsString = BogusCosmosDbAccount.Documents.Query<TestObject>()
                                                .Where(x => x.SomeIntProperty == 20)
                                                .ToString().ToLower();

            Assert.Contains("where", testObjectsQueryAsString);
            Assert.Contains("someintproperty", testObjectsQueryAsString);
            Assert.Contains(testValue.ToString(), testObjectsQueryAsString);

            var testObjectsQueryWithVariableAsString = BogusCosmosDbAccount.Documents.Query<TestObject>()
                                                            .Where(x => x.SomeIntProperty == testValue)
                                                            .ToString().ToLower();

            Assert.Contains("where", testObjectsQueryWithVariableAsString);
            Assert.Contains("someintproperty", testObjectsQueryWithVariableAsString);
            Assert.Contains(testValue.ToString(), testObjectsQueryWithVariableAsString);
        }

        [Fact]
        public void WhereClauseWithGuid()
        {
            var testObjectsQueryAsString = BogusCosmosDbAccount.Documents.Query<TestObject>()
                                                .Where(x => x.GuidId == Guid.Empty)
                                                .ToString().ToLower();

            Assert.NotNull(testObjectsQueryAsString);
        }
    }
}
