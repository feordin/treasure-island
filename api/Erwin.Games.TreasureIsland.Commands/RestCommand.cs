using Erwin.Games.TreasureIsland.Actions;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class RestCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly string _commandName;

        public RestCommand(SaveGameData? saveGameData, string commandName)
        {
            _saveGameData = saveGameData;
            _commandName = commandName;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            ProcessCommandResponse? response;

            
            response = new ProcessCommandResponse(
                message: "You manage to find a spot to hunker down for a few minutes. You are sure you will feel better after a bit of a rest.",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null);

            if (_saveGameData != null)
                _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddMinutes(30);

            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            if (currentLocation?.Name?.Contains("BoatCabin", StringComparison.OrdinalIgnoreCase) == true)
            {
                var shipWreckAction = new ShipWreckAction(response);
                shipWreckAction.Execute();
            }

            return Task.FromResult<ProcessCommandResponse?>(response);
        }
    }
}