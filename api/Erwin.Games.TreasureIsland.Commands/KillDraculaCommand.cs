using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class KillDraculaCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly string? _target;

        public KillDraculaCommand(SaveGameData? saveGameData, string? target = null)
        {
            _saveGameData = saveGameData;
            _target = target;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null || WorldData.Instance == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Unable to get the current game state or world data.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if target is valid (null, empty, or dracula/vampire)
            if (!string.IsNullOrEmpty(_target) &&
                !_target.Equals("dracula", StringComparison.OrdinalIgnoreCase) &&
                !_target.Equals("vampire", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: $"There is nothing called {_target} here to kill.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if Dracula is already killed
            var killedEvent = _saveGameData.GetEvent("killed_dracula");
            if (killedEvent != null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Dracula is already dead",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if player has wooden stake and hammer
            var hasWoodenStake = _saveGameData.Inventory?.Contains("woodenStake", StringComparer.OrdinalIgnoreCase) == true;
            var hasHammer = _saveGameData.Inventory?.Contains("hammer", StringComparer.OrdinalIgnoreCase) == true;

            if (!hasWoodenStake || !hasHammer)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You need a wooden stake and hammer to kill Dracula",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if player is in the CoffinRoom
            if (!_saveGameData.CurrentLocation?.Equals("CoffinRoom", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Dracula is not here",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if it's daytime (hour 6-20) - must match DraculaAction's night check
            var currentHour = _saveGameData.CurrentDateTime.Hour;
            if (currentHour < 6 || currentHour >= 20)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Dracula is awake and you cannot approach him!",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // All conditions met - kill Dracula
            _saveGameData.AddEvent("killed_dracula", "You drive the wooden stake through Dracula's heart with the hammer. The vampire lord crumbles to dust.", _saveGameData.CurrentDateTime);

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "You drive the wooden stake through Dracula's heart with the hammer. The vampire lord crumbles to dust.",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }
    }
}
