using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Actions;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class RubCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly string? _target;

        public RubCommand(SaveGameData? saveGameData, string? target)
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

            // Check what we're trying to rub
            if (string.IsNullOrEmpty(_target) ||
                (!_target.Equals("lamp", StringComparison.OrdinalIgnoreCase) &&
                 !_target.Equals("aladdins", StringComparison.OrdinalIgnoreCase) &&
                 !_target.Equals("aladdinslamp", StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "What do you want to rub?",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if player has the lamp
            bool hasLamp = _saveGameData.Inventory?.Any(item =>
                item.Equals("aladdinsLamp", StringComparison.OrdinalIgnoreCase) ||
                item.Equals("lamp", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasLamp)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You don't have Aladdin's lamp.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if lamp has already been used
            if (_saveGameData.GetEvent("lamp_used") != null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "The lamp has already been used. The genie only grants one wish.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if in FissureRoom or FissureLedge
            bool inFissureRoom = _saveGameData.CurrentLocation?.Equals("FissureRoom", StringComparison.OrdinalIgnoreCase) == true;
            bool inFissureLedge = _saveGameData.CurrentLocation?.Equals("FissureLedge", StringComparison.OrdinalIgnoreCase) == true;

            if (!inFissureRoom && !inFissureLedge)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You rub the lamp but nothing happens. Perhaps it only works in a special place...",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Mark lamp as used
            _saveGameData.AddEvent("lamp_used", "Used Aladdin's lamp for teleportation", _saveGameData.CurrentDateTime);

            // Check if player has King Tut's treasure
            bool hasTreasure = _saveGameData.Inventory?.Any(item =>
                item.Equals("kingsTutTreasure", StringComparison.OrdinalIgnoreCase)) ?? false;

            string message;
            string? newLocation;
            string? locationDesc;

            if (inFissureRoom)
            {
                // Teleport to FissureLedge
                newLocation = "FissureLedge";
                message = "You rub the lamp and a genie appears in a puff of smoke! 'Your wish is my command!' In a flash of light, you find yourself on the narrow ledge on the other side of the fissure.";

                if (_saveGameData.GetEvent("fissure_filled") == null)
                {
                    // Player used lamp to cross without water — try monkey's paw escape
                    _saveGameData.PreviousLocation = "FissureRoom";
                    var trappedResponse = new ProcessCommandResponse(
                        message: message,
                        saveGameData: _saveGameData,
                        imageFilename: null,
                        locationDescription: null,
                        commandHistory: null);

                    if (LastChanceEscape.TryEscape(trappedResponse, "trapped on fissure ledge"))
                    {
                        return Task.FromResult<ProcessCommandResponse?>(trappedResponse);
                    }

                    // No escape — they're stuck
                    message += "\n\nBut wait... without water in the fissure, you have no way to swim back. And the lamp only grants one wish. You are trapped!";
                    _saveGameData.AddEvent("GameOver", "Trapped on fissure ledge", _saveGameData.CurrentDateTime);
                }
            }
            else // inFissureLedge
            {
                // Teleport back to FissureRoom
                newLocation = "FissureRoom";

                if (hasTreasure)
                {
                    message = "You rub the lamp and a genie appears in a puff of smoke! 'Your wish is my command!' In a flash of light, you find yourself back on the original side of the fissure, with King Tut's treasure safely in hand!";
                }
                else
                {
                    message = "You rub the lamp and a genie appears in a puff of smoke! 'Your wish is my command!' In a flash of light, you find yourself back on the original side of the fissure. But wait - you forgot to take the treasure! And the lamp only works once...";
                }
            }

            _saveGameData.CurrentLocation = newLocation;
            _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddMinutes(1);

            var destLocation = WorldData.Instance.Locations?.FirstOrDefault(l =>
                l.Name?.Equals(newLocation, StringComparison.OrdinalIgnoreCase) == true);
            locationDesc = destLocation?.Description;

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: message,
                saveGameData: _saveGameData,
                imageFilename: destLocation?.Image,
                locationDescription: locationDesc,
                commandHistory: null));
        }
    }
}
