using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

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
            // Read the body of the POST request
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            JObject data = JsonConvert.DeserializeObject<JObject>(requestBody);

            // Example: Access a property from the JSON body
            string command = data["command"].ToString();

            _logger.LogInformation("Processing game command: {0}", command);
           
            return new OkObjectResult(string.Format("Processed game command: {0}", command));
        }

        private async Task<string> GetStartingLocationDescription()
        {
            var container = _cosmosClient.GetContainer(_databaseId, _containerId);
            var response = await container.ReadItemAsync<dynamic>("startinglocationdescription", new PartitionKey("startinglocationdescription"));
            var document = response.Resource;
            var returnValue = ((Newtonsoft.Json.Linq.JObject)document).Value<string>("description");
            return returnValue;
        }
    }
}
