using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class SleepCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;

        public SleepCommand(SaveGameData? saveGameData)
        {
            _saveGameData = saveGameData;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            ProcessCommandResponse? response;

            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            if (currentLocation?.Name?.Contains("street", StringComparison.OrdinalIgnoreCase) == true ||
                currentLocation?.Name?.Contains("ave", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (_saveGameData != null)
                    _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddMinutes(60);
                response = new ProcessCommandResponse(
                    message: "You suddenly wake up to the angry voice of the watch. You are warned that sleeping in the street is not allowed. You quickly gather your things and move on.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null);
            }
            else
            {
                if (_saveGameData != null)
                    _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddHours(8);
                response = new ProcessCommandResponse(
                    message: "You manage to get comfortable and fall asleep. You wake up feeling refreshed and ready to continue your adventure.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null);
            }

            return Task.FromResult<ProcessCommandResponse?>(response);
        }
    }
}