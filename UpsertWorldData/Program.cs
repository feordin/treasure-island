using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

public partial class Program
{
    private static string? endpoint = Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT");
    private static string? key = Environment.GetEnvironmentVariable("COSMOSDB_KEY");
    private static string databaseId = "treasureisland";
    private static string containerId = "gamedata";
    private static string settingsFilePath = "../api/local.settings.json";

    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No file path provided.");
            return;
        }

        string filePath = args[0];

        // Read the local settings file if endpoint or key is null
        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
        {
            var settings = JObject.Parse(File.ReadAllText(settingsFilePath));
            if (string.IsNullOrEmpty(endpoint))
            {
                endpoint = settings["Values"]["CosmosDBEndpoint"].ToString();
            }
            if (string.IsNullOrEmpty(key))
            {
                key = settings["Values"]["CosmosDBKey"].ToString();
            }
        }

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
        {
            Console.WriteLine("No endpoint or key provided.");
            return;
        }

        Console.WriteLine($"Endpoint: {endpoint}");

        // Create a new instance of the CosmosClient
        CosmosClient cosmosClient = new CosmosClient(endpoint, key);

        // Get a reference to the database and container
        Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/id");

        // Read the JSON file
        string jsonContent = File.ReadAllText(filePath);
        JObject document = JObject.Parse(jsonContent);

        // Upsert the document
        await container.UpsertItemAsync(document);
        Console.WriteLine($"Document upserted successfully to database '{databaseId}' and container '{containerId}'.");
    }
}
