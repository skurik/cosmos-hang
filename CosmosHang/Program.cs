using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Newtonsoft.Json;

namespace CosmosHang
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var connectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            var clientBuilder = new CosmosClientBuilder(connectionString);
            var clientWithSerialization = clientBuilder.WithConnectionModeGateway().WithSerializerOptions(new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            });
            var containerProperties = new ContainerProperties
            {
                Id = "testcontainer",
                PartitionKeyPath = $"/partitionKey"
            };

            var client = clientWithSerialization.Build();
            var database = (await client.CreateDatabaseIfNotExistsAsync("test")).Database;
            var container = (await database.CreateContainerIfNotExistsAsync(containerProperties)).Container;

            var item = new Item
            {
                Id = Guid.NewGuid(),
                PartitionKey = "abc"
            };

            // await container.CreateItemAsync(item);

            var batch = container.CreateTransactionalBatch(new PartitionKey(item.PartitionKey));
            batch.CreateItem(item);
            await batch.ExecuteAsync();
        }

        public class Item
        {
            [JsonProperty("id")]
            public Guid Id { get; set; }
            [JsonProperty("partitionKey")]
            public string PartitionKey { get; set; }
        }
    }
}