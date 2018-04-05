using Stellar;
using Xunit;

namespace Tests
{
    public class SimpleQueryTest
    {
        private CosmosDbAccount BogusCosmosDbAccount => new CosmosDbAccount(@"http://www.bogusendpoint.com", "bogustoken", "bogusdb", "");

        [Fact]
        public void SimpleQueryShouldIncludeSelectAndType()
        {
            var testObjectsQueryAsString = BogusCosmosDbAccount.Documents.Query<TestObject>().ToString().ToLower();

            Assert.Contains("select", testObjectsQueryAsString);
            Assert.Contains("*", testObjectsQueryAsString);
            Assert.Contains("from", testObjectsQueryAsString);
            Assert.Contains(typeof(TestObject).Name.ToString().ToLower(), testObjectsQueryAsString);
            Assert.Contains("as", testObjectsQueryAsString);
        }

        [Fact]
        public void SimpleQueryShouldIncludeSelectAndTypeAndTypePredicate()
        {
            var testObjectsQueryAsString = BogusCosmosDbAccount.Documents.Query<TestObject>().ToString().ToLower();

            Assert.Contains("select", testObjectsQueryAsString);
            Assert.Contains("*", testObjectsQueryAsString);
            Assert.Contains("from", testObjectsQueryAsString);
            Assert.Contains(typeof(TestObject).Name.ToString().ToLower(), testObjectsQueryAsString);
            Assert.Contains("as", testObjectsQueryAsString);
            Assert.Contains("_type", testObjectsQueryAsString);
            Assert.Contains(typeof(TestObject).FullName.ToString().ToLower(), testObjectsQueryAsString);
        }
    }
}
