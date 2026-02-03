using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Commands;
using Erwin.Games.TreasureIsland.Persistence;
using Erwin.Games.TreasureIsland.Actions;

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
            string? parsedCommand = command;

            if (command != "startup")
            {
                // Here we analyze the input, perhaps with an LLM
                // Then the cleaned up command is fed to a factory to create a command object
                // Then we execute the command object
                parsedCommand = await _aiClient.ParsePlayerInputWithAgent(command);
                _logger.LogInformation("Parsed game command: {0}", parsedCommand);
            }

            ICommand cmd = CommandFactory.CreateCommand(parsedCommand, data.saveGameData, _gameDataRepository);

            var result = await cmd.Execute();

            // now check any required actions based on our current location
            var actions = WorldData.Instance?.GetLocation(result?.saveGameData?.CurrentLocation)?.Actions;
            if (actions != null && result != null)
            {
                foreach (var action in actions)
                {
                    var actionObject = ActionFactory.CreateAction(action, result);
                    if (actionObject != null)
                    {
                        actionObject.Execute();
                    }
                }
            }

            // update the history
            await _gameDataRepository.AddToGameHistory(command, result?.Message, result?.saveGameData?.Player, 0);

            // update the autosave game
            await _gameDataRepository.SaveGameAsync(result?.saveGameData, 0);

            // if there is nothing populated in savedGame list yet, add the autosave
            if (result?.saveGameData != null && result?.SavedGames?.Count == 0)
            {
                var saveGameList = await _gameDataRepository.GetAllSavedGamesAsync(ClientPrincipal.Instance?.UserDetails);
                if (saveGameList == null && result?.saveGameData != null)
                {
                    saveGameList = new List<SaveGameData>() { result.saveGameData };
                }
                
                if (saveGameList != null)
                {
                    result?.SavedGames?.AddRange(saveGameList);
                }
            }

            return new OkObjectResult(result);
        }
    }
}
