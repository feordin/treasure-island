using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class LightCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly IGameDataRepository _repository;
        private readonly string? _target;

        public LightCommand(SaveGameData? saveGameData, IGameDataRepository repository, string? target)
        {
            _saveGameData = saveGameData;
            _repository = repository;
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

            // Check what we're trying to light
            if (string.IsNullOrEmpty(_target))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "What do you want to light?",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // If target is a fuel item and player is at Rescue Beach, redirect to signal command
            var fuelItems = new[] { "driftwood", "lumber", "coal" };
            if (fuelItems.Any(f => _target.Equals(f, StringComparison.OrdinalIgnoreCase) ||
                    _target.StartsWith(f, StringComparison.OrdinalIgnoreCase)) &&
                _saveGameData.CurrentLocation?.Equals("RescueBeach", StringComparison.OrdinalIgnoreCase) == true)
            {
                var signalCommand = new SignalCommand(_saveGameData, _repository, "signal");
                return signalCommand.Execute();
            }

            // Handle lighting coal
            if (_target.Equals("coal", StringComparison.OrdinalIgnoreCase))
            {
                return LightCoal();
            }

            // Handle lighting matches
            if (_target.Equals("matches", StringComparison.OrdinalIgnoreCase) ||
                _target.Equals("match", StringComparison.OrdinalIgnoreCase))
            {
                return LightMatches();
            }

            // Handle "light fire" / "build fire" etc.
            if (_target.Equals("fire", StringComparison.OrdinalIgnoreCase))
            {
                return LightFire();
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: $"You can't light the {_target}.",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> LightCoal()
        {
            // Check if player has coal
            bool hasCoal = _saveGameData!.Inventory?.Any(item =>
                item.Equals("coal", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasCoal)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You don't have any coal to light.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if player has matches
            bool hasMatches = _saveGameData.Inventory?.Any(item =>
                item.Equals("matches", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasMatches)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You need matches to light the coal.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if matches are wet
            if (_saveGameData.GetEvent("wet_matches") != null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Your matches are wet and won't light!",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if in an ice cave (WestIceCave or EastIceCave)
            bool isInIceCave = _saveGameData.CurrentLocation?.Equals("WestIceCave", StringComparison.OrdinalIgnoreCase) == true ||
                               _saveGameData.CurrentLocation?.Equals("EastIceCave", StringComparison.OrdinalIgnoreCase) == true;

            if (!isInIceCave)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "There's no point lighting coal here. You need to be somewhere the heat would be useful.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if already melted ice
            if (_saveGameData.GetEvent("ice_melted") != null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You've already melted the ice here.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Success - melt the ice!
            _saveGameData.AddEvent("ice_melted", "Melted ice in west ice cave", _saveGameData.CurrentDateTime);

            // Remove coal from inventory (it's consumed)
            _saveGameData.Inventory?.Remove("coal");

            // Check if player has canteen to fill with water
            bool hasCanteen = _saveGameData.Inventory?.Any(item =>
                item.Equals("canteen", StringComparison.OrdinalIgnoreCase)) ?? false;

            string message = "The coal begins to burn brightly and the fire melts the ice around you! Water streams down the cave walls.";

            if (hasCanteen)
            {
                _saveGameData.AddEvent("canteen_filled", "Filled canteen with melted ice water", _saveGameData.CurrentDateTime);
                message += " You quickly fill your canteen with the fresh, cold water.";
            }

            message += "\n\nThe melted ice flows through cracks in the rock, filling the fissure in the nearby cavern with water.";

            // Add dynamic movement to FissureRoom - now player can swim west
            var fissureRoom = WorldData.Instance.Locations?.FirstOrDefault(l =>
                l.Name?.Equals("FissureRoom", StringComparison.OrdinalIgnoreCase) == true);

            if (fissureRoom != null)
            {
                // The movement to WestFissureRoom is now accessible via swimming
                _saveGameData.AddEvent("fissure_filled", "Fissure filled with water from melted ice", _saveGameData.CurrentDateTime);
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: message,
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> LightMatches()
        {
            bool hasMatches = _saveGameData!.Inventory?.Any(item =>
                item.Equals("matches", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasMatches)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You don't have any matches.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            if (_saveGameData.GetEvent("wet_matches") != null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Your matches are wet and won't light!",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "You strike a match. It flares briefly in the darkness, casting dancing shadows on the walls before burning out.",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> LightFire()
        {
            // Check if player has matches
            bool hasMatches = _saveGameData!.Inventory?.Any(item =>
                item.Equals("matches", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasMatches)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You need matches to light a fire.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if matches are wet
            if (_saveGameData.GetEvent("wet_matches") != null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Your matches are wet and won't light!",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check for fuel
            bool hasCoal = _saveGameData.Inventory?.Any(item =>
                item.Equals("coal", StringComparison.OrdinalIgnoreCase)) ?? false;
            bool hasDriftwood = _saveGameData.Inventory?.Any(item =>
                item.Equals("driftwood", StringComparison.OrdinalIgnoreCase)) ?? false;
            bool hasLumber = _saveGameData.Inventory?.Any(item =>
                item.Equals("lumber", StringComparison.OrdinalIgnoreCase)) ?? false;

            bool hasFuel = hasCoal || hasDriftwood || hasLumber;

            if (!hasFuel)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You have matches but nothing to burn. You need fuel like coal, driftwood, or lumber.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check location - ice caves use coal to melt ice
            bool isInIceCave = _saveGameData.CurrentLocation?.Equals("WestIceCave", StringComparison.OrdinalIgnoreCase) == true ||
                               _saveGameData.CurrentLocation?.Equals("EastIceCave", StringComparison.OrdinalIgnoreCase) == true;

            if (isInIceCave)
            {
                if (!hasCoal)
                {
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                        message: "You have fuel but it won't burn hot enough to melt the ice. You need coal.",
                        saveGameData: _saveGameData,
                        imageFilename: null,
                        locationDescription: null,
                        commandHistory: null));
                }

                // Redirect to LightCoal for ice melting logic
                return LightCoal();
            }

            // Check if at RescueBeach - redirect to signal command
            if (_saveGameData.CurrentLocation?.Equals("RescueBeach", StringComparison.OrdinalIgnoreCase) == true)
            {
                var signalCommand = new SignalCommand(_saveGameData, _repository, "signal");
                return signalCommand.Execute();
            }

            // Not in a useful location
            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "This doesn't seem like a useful place to light a fire. Perhaps at a beach to signal for rescue, or in an icy cave where the heat could melt something?",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }
    }
}
