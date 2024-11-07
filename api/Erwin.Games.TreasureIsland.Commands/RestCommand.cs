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

            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            if (currentLocation?.Name?.Contains("street", StringComparison.OrdinalIgnoreCase) == true ||
                currentLocation?.Name?.Contains("ave", StringComparison.OrdinalIgnoreCase) == true)
            {
                response = new ProcessCommandResponse(
                    message: "You take a nice break.  You feel refreshed and ready to continue your adventure.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null);
            }
            else
            {
                response = new ProcessCommandResponse(
                    message: "You look around for a place to rest, but don't find anything suitable.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null);
            }

            if (_saveGameData != null)
                _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddMinutes(30);

            return Task.FromResult<ProcessCommandResponse?>(response);
        }
    }
}