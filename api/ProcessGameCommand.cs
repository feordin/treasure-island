using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Erwin.Games.TreasureIsland
{
    public class ProcessGameCommand
    {
        private readonly ILogger<ProcessGameCommand> _logger;

        public ProcessGameCommand(ILogger<ProcessGameCommand> logger)
        {
            _logger = logger;
        }

        [Function("ProcessGameCommand")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
