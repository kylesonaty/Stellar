using System;
using System.Linq;
using Tests;
using Xunit;

namespace Stellar.Tests
{
    public class SimpleQueryWithWhere
    {
        private CosmosDbAccount BogusCosmosDbAccount => new CosmosDbAccount(@"http://www.bogusendpoint.com", "Ym9ndXN0b2tlbg==", "bogusdb", "");

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

        [Fact]
        public void WhereWithNullable()
        {
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.NullableGuid.Value == Guid.Empty);

            var queryString = query.ToString();
            Assert.NotNull(queryString);
            Assert.Contains(".nullableGuid = '0000", queryString);
        }

        [Fact]
        public void WhereWithGeneric()
        {
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.IdName.Id == 1);
            var queryString = query.ToString().ToLower();
            Assert.Contains(".idname.id = 1", queryString);
        }

        [Fact]
        public void WhereWithContains()
        {
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.Ids.Contains(1));
            var queryString = query.ToString();
            Assert.Contains("ARRAY_CONTAINS(t0.ids, 1)", queryString);
        }


        [Fact]
        public void WhereWithContainsWithVariable()
        {
            var id = 1;
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.Ints.Contains(id));
            var queryString = query.ToString();
            Assert.Contains("ARRAY_CONTAINS(t0.ints, 1)", queryString);
        }

        [Fact]
        public void WhereWithArrayLength()
        {
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.Ints.Count() > 0);
            var queryString = query.ToString();
            Assert.Contains("ARRAY_LENGTH(t0.ints)", queryString);
        }

        [Fact]
        public void WhereWithOrElse()
        {
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.Name == "test"
                            && (x.PossibleDateTime == DateTime.MinValue
                            || x.SomeIntProperty == 1));
            var queryString = query.ToString();
            Assert.Contains("OR", queryString);
        }

        [Fact]
        public void WhereWithBoolTrue()
        {
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.Name == "test"
                            && (x.PossibleDateTime == DateTime.MinValue
                            || x.IsBool == true));
            var queryString = query.ToString();
            Assert.Contains("OR", queryString);
            Assert.Contains("true", queryString);
        }

        [Fact]
        public void WhereWithBoolFalse()
        {
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.Name == "test"
                            && (x.PossibleDateTime == DateTime.MinValue
                            || x.IsBool == false));
            var queryString = query.ToString();
            Assert.Contains("OR", queryString);
            Assert.Contains("false", queryString);
        }

        [Fact]
        public void WhereWithDateTime()
        {
            var dt = new DateTime(1985, 1, 11, 6, 24, 48);
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.Name == "test"
                            && (x.PossibleDateTime == dt
                            || x.IsBool == false));
            var queryString = query.ToString();
            var dtString = dt.ToString("O");
            Assert.Contains("OR", queryString);
            Assert.Contains("false", queryString);
            Assert.Contains(dtString, queryString);
        }

        [Fact]
        public void WhereWithDateTimeOffset()
        {
            var dt = new DateTimeOffset(1985, 1, 11, 6, 24, 48, TimeSpan.Zero);
            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
                            .Where(x => x.Name == "test"
                            && (x.PossibleDateTimeOffset == dt
                            || x.IsBool == false));
            var queryString = query.ToString();
            var dtString = dt.ToString("O");
            Assert.Contains("OR", queryString);
            Assert.Contains("false", queryString);
            Assert.Contains(dtString, queryString);
        }
    }
}