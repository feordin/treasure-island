using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class TurnCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly string? _action; // "on" or "off"
        private readonly string? _target; // what to turn on/off

        public TurnCommand(SaveGameData? saveGameData, string? action, string? target)
        {
            _saveGameData = saveGameData;
            _action = action;
            _target = target;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Unable to get the current game state.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            if (string.IsNullOrEmpty(_target))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: $"What do you want to turn {_action}?",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Handle flashlight
            if (_target.Equals("flashlight", StringComparison.OrdinalIgnoreCase) ||
                _target.Equals("light", StringComparison.OrdinalIgnoreCase) ||
                _target.Equals("torch", StringComparison.OrdinalIgnoreCase))
            {
                return HandleFlashlight();
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: $"You can't turn the {_target} {_action}.",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> HandleFlashlight()
        {
            // Check if player has the flashlight
            bool hasFlashlight = _saveGameData!.Inventory?.Any(item =>
                item.Equals("flashlight", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasFlashlight)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You don't have a flashlight.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            bool isOn = _saveGameData.GetEvent("flashlight_on") != null;

            if (_action?.Equals("on", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (isOn)
                {
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                        message: "The flashlight is already on.",
                        saveGameData: _saveGameData,
                        imageFilename: null,
                        locationDescription: null,
                        commandHistory: null));
                }

                _saveGameData.AddEvent("flashlight_on", "Flashlight turned on", _saveGameData.CurrentDateTime);
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You turn on the flashlight. A bright beam of light cuts through the darkness.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }
            else if (_action?.Equals("off", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (!isOn)
                {
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                        message: "The flashlight is already off.",
                        saveGameData: _saveGameData,
                        imageFilename: null,
                        locationDescription: null,
                        commandHistory: null));
                }

                _saveGameData.RemoveEvent("flashlight_on");
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You turn off the flashlight. Darkness surrounds you.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "Do you want to turn the flashlight on or off?",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }
    }
}
