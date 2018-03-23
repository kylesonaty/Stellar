using Stellar;
using Xunit;

namespace Tests
{
    public class SimpleQueryTest
    {
        private CosmosDbAccount BogusCosmosDbAccount => new Stellar.CosmosDbAccount(@"http://www.bogusendpoint.com", "bogustoken", "bogusdb", "");

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
    }
}
