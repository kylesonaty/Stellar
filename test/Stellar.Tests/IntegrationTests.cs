//using Stellar.Samples;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Tests;
//using Xunit;

//namespace Stellar.Tests
//{
//    public class IntegrationTests
//    {
//        private CosmosDbAccount BogusCosmosDbAccount => new CosmosDbAccount(@"https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", "FamilyDB", "kids");

//        [Fact]
//        public void WhereWithNoMatchToList()
//        {
//            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
//                    .Where(x => x.Name == "thisshouldnotwork");
//            var list = query.ToList();
//            Assert.NotNull(list);
//        }

//        [Fact]
//        public void WhereWithNoMatchToListAsync()
//        {
//            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
//                    .Where(x => x.Name == "thisshouldnotwork");
//            var list = query.ToListAsync().Result;
//            Assert.NotNull(list);

//            var result = GetAsync().Result;
//            Assert.Null(result);
//        }

//        public async Task<TestObject> GetAsync()
//        {
//            var query = BogusCosmosDbAccount.Documents.Query<TestObject>()
//                    .Where(x => x.Name == "thisshouldnotwork");
//            var list = await query.ToListAsync();
//            return list.FirstOrDefault();
//        }

//        [Fact]
//        public void Add1000TestObjects()
//        {
//            var db = BogusCosmosDbAccount.Documents;
//            var item = new Item
//            {
//                Guid = Guid.NewGuid(),
//                Name = "test"
//            };

//            for (int i = 0; i < 100; i++)
//            {
//                item.Guid = Guid.NewGuid();
//                var result = db.Store(item.Id, item.Name, item).Result;
//            }
//            Assert.True(true);
//        }

//        [Fact]
//        public void GetItAll()
//        {
//            var db = BogusCosmosDbAccount.Documents;
//            var query = db.Query<Item>().Where(x => x.Name == "test");
//            var list = query.ToList();
//            Assert.True(list.Count > 100);
//        }

//        [Fact]
//        public void GetDateTime()
//        {
//            var db = BogusCosmosDbAccount.Documents;
//            var query = db.Query<TestObject>().Where(x => x.Name == "test" && x.DateTime > DateTime.Now.AddHours(-1));
//            var list = query.ToList();
//            Assert.Empty(list);
//        }

//        [Fact]
//        public void GetDateTimeOffset()
//        {
//            var db = BogusCosmosDbAccount.Documents;
//            var query = db.Query<TestObject>().Where(x => x.Name == "test" && x.DateTimeOffset > DateTimeOffset.Now.AddHours(-1));
//            var list = query.ToList();
//            Assert.Empty(list);
//        }

//        [Fact]
//        public void GetPossibleDateTime()
//        {
//            var db = BogusCosmosDbAccount.Documents;
//            var query = db.Query<TestObject>().Where(x => x.Name == "test" && x.PossibleDateTime > DateTime.Now.AddHours(-1));
//            var list = query.ToList();
//            Assert.Empty(list);
//        }

//        [Fact]
//        public void GetPossibleDateTimeOffset()
//        {
//            var db = BogusCosmosDbAccount.Documents;
//            var query = db.Query<TestObject>().Where(x => x.Name == "test" && x.PossibleDateTimeOffset > DateTimeOffset.Now.AddHours(-1));
//            var list = query.ToList();
//            Assert.Empty(list);
//        }

//    }

//    public class Item
//    {
//        public string Id { get => Guid.ToString(); }
//        public Guid Guid { get; set; }
//        public string Name { get; set; }
//    }

//    public static class CosmosQueryableExtesions
//    {
//        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> source)
//        {
//            return await Task.FromResult(source.ToList());
//        }
//    }
//}
