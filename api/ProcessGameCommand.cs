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
                // Fast path: Try direct command matching first (no AI needed)
                parsedCommand = CommandMatcher.TryMatchCommand(command);

                if (parsedCommand != null)
                {
                    _logger.LogInformation("Fast path matched: {0} -> {1}", command, parsedCommand);
                }
                else
                {
                    // Slow path: Use AI for natural language understanding
                    // Pass location context to help with commands like "enter the bank"
                    var currentLocation = WorldData.Instance?.GetLocation(data.saveGameData?.CurrentLocation);
                    parsedCommand = await _aiClient.ParsePlayerInputWithAgent(command, currentLocation, data.saveGameData);
                    _logger.LogInformation("AI parsed: {0} -> {1}", command, parsedCommand);
                }
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

            // Global Dracula action - runs on every command
            if (result != null && result.saveGameData?.GetEvent("GameOver") == null)
            {
                var draculaAction = new DraculaAction(result);
                draculaAction.Execute();
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

            // Populate the display name for the current location
            if (result?.saveGameData != null)
            {
                var location = WorldData.Instance?.GetLocation(result.saveGameData.CurrentLocation);
                result.saveGameData.CurrentLocationDisplayName = location?.GetDisplayName();
            }

            return new OkObjectResult(result);
        }
    }
}
