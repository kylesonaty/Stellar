using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Stellar.Samples
{
    class Program
    {
        private const string
            EndpointUrl = "https://localhost:8081",
            AuthKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            DatabaseId = "FamilyDB",
            CollectionId = "FamilyCollection";

        static async Task Main(string[] args)
        {
            try
            {
                await EnsureCosmosDbExists(DatabaseId, CollectionId);

                var docs = new CosmosDbAccount(EndpointUrl, AuthKey, DatabaseId, CollectionId).Documents;
                var family = GetExampleFamily();

                Console.WriteLine("Storing new family.");
                var response = await docs.Store(family.Id.ToString(), family);
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Body: {response.Body}");
                Console.WriteLine(new string('-', Console.WindowWidth));

                var families = await docs.Query<Family>("SELECT * FROM c WHERE c.lastName = 'Andersen' ORDER BY c.address.state");
                Console.WriteLine($"Found {families.Count()} Andersen families.");

                families = await docs.Query<Family>("SELECT * FROM c ORDER BY c.address.state");
                Console.WriteLine($"Found {families.Count()} families ordered by state.");

                families = await docs.Query<Family>("SELECT * FROM c");
                Console.WriteLine($"Found {families.Count()} total families.");


                Console.WriteLine("Retrieving stored family.");
                var storedFamily = await docs.Get<Family>(family.Id);
                Console.WriteLine($"Family ID: {family.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oops, something went wrong.");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Trace: {ex.StackTrace}");
            }

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }

        /// <summary>
        /// TODO: Replace with Stellar code once Stellar can create databases
        /// </summary>
        /// <param name="client"></param>
        /// <param name="databaseId"></param>
        static async Task EnsureCosmosDbExists(string databaseId, string collectionId = null)
        {
            // Only required temporarily until Stellar can create databases and collections
            DocumentClient client = new DocumentClient(new Uri(EndpointUrl), AuthKey);

            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });
            if (!string.IsNullOrWhiteSpace(collectionId))
                await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseId), new DocumentCollection { Id = collectionId });
        }

        static Family GetExampleFamily(string Id = null)
        {
            return new Family
            {
                Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString() : Id,
                LastName = "Andersen",
                Parents = new Parent[]
                {
                    new Parent { FirstName = "Thomas" },
                    new Parent { FirstName = "Mary Kay" }
                },
                Children = new Child[]
                {
                    new Child
                    {
                        FirstName = "Henriette Thaulow",
                        Gender = "female",
                        Grade = 5,
                        Pets = new Pet[] { new Pet { GivenName = "Fluffy" } }
                    }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = true
            };
        }
    }
}