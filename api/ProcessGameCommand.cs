using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Azure.Identity;

namespace Erwin.Games.TreasureIsland
{
    public class ProcessGameCommand
    {
        private readonly ILogger<ProcessGameCommand> _logger;
        private static Lazy<CosmosClient> _lazyClient = new Lazy<CosmosClient>(InitializeCosmosClient);
        private static CosmosClient _cosmosClient => _lazyClient.Value;
        private static readonly string _databaseId = "treasureisland";
        private static readonly string _containerId = "gamedata";

        private static CosmosClient InitializeCosmosClient()
        {
            // Perform any initialization here
            var uri = Environment.GetEnvironmentVariable("CosmosDBEndpoint");
            var key = Environment.GetEnvironmentVariable("CosmosDBKey");
            return new CosmosClient(uri, key);
        }

        public ProcessGameCommand(ILogger<ProcessGameCommand> logger)
        {
            _logger = logger;
        }

        [Function("ProcessGameCommand")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var container = _cosmosClient.GetContainer(_databaseId, _containerId);
            var response = await container.ReadItemAsync<dynamic>("startinglocationdescription", new PartitionKey("startinglocationdescription"));
            var document = response.Resource;
            var returnValue = ((Newtonsoft.Json.Linq.JObject)document).Value<string>("description");
            
            return new OkObjectResult(returnValue);
        }
    }
}
