using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class DrinkCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly string? _target;

        public DrinkCommand(SaveGameData? saveGameData, string? target)
        {
            _saveGameData = saveGameData;
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

            // Check if player has canteen
            bool hasCanteen = _saveGameData.Inventory?.Any(item =>
                item.Equals("canteen", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasCanteen)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You don't have anything to drink from.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if canteen has water
            if (_saveGameData.GetEvent("canteen_filled") == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Your canteen is empty. You need to fill it with water first.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Drink the water - remove thirsty event if present
            var thirstyEvent = _saveGameData.GetEvent("thirsty");
            string message;

            if (thirstyEvent != null)
            {
                _saveGameData.RemoveEvent("thirsty");
                message = "You drink deeply from the canteen. The cool water quenches your terrible thirst. You feel much better!";
            }
            else
            {
                message = "You take a refreshing drink from the canteen. The water is cool and satisfying.";
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: message,
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }
    }
}
