using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Commands;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland
{
    public class ProcessGameCommand
    {
        private readonly ILogger<ProcessGameCommand> _logger;
        private readonly IGameDataRepository _gameDataRepository;
        private readonly IAIClient _aiClient;

        public ProcessGameCommand(ILogger<ProcessGameCommand> logger, IGameDataRepository gameDataRepository, IAIClient aiClient)
        {
            _logger = logger;
            _gameDataRepository = gameDataRepository;
            _aiClient = aiClient;
        }

        [Function("ProcessGameCommand")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            ClientPrincipal.Instance = ClientPrincipal.Parse(req);
            _logger.LogInformation("ClientPrincipal: {0}", ClientPrincipal.Instance);

            if (WorldData.Instance == null)
            {
                WorldData.Instance = await _gameDataRepository.LoadWorldDataAsync();
            }

            // Read the body of the POST request
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CommandRequest>(requestBody);

            // Example: Access a property from the JSON body
            string? command = data.command;

            _logger.LogInformation("Processing game command: {0}", command);

            var parsedCommand = await _aiClient.ParsePlayerInput(command);

            _logger.LogInformation("Parsed game command: {0}", parsedCommand);

            // Here we analyze the input, perhaps with an LLM
            // Then the cleaned up command is fed to a factory to create a command object
            // Then we execute the command object
            ICommand cmd = CommandFactory.CreateCommand(parsedCommand, data.saveGameData, _gameDataRepository);

            var result = await cmd.Execute();

            // as the final part of each command, we update the game state
            // 1) update the player's location
            // 2) update the player's inventory
            // 3) update the player's score
            // 4) update the player's health
            // 5) update the game date/time

            return new OkObjectResult(result);
        }
    }
}
