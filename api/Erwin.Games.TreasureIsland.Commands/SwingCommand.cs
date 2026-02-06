using System;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    /// <summary>
    /// Command to swing across the creek using the jungle vine.
    /// Works at both Creek and SouthCreek locations.
    /// </summary>
    public class SwingCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly IGameDataRepository _repository;
        private readonly string? _target;

        public SwingCommand(SaveGameData? saveGameData, IGameDataRepository repository, string? target)
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

            var currentLocation = _saveGameData.CurrentLocation;

            // Check if we're at a creek location
            bool atCreek = currentLocation?.Equals("Creek", StringComparison.OrdinalIgnoreCase) == true;
            bool atSouthCreek = currentLocation?.Equals("SouthCreek", StringComparison.OrdinalIgnoreCase) == true;

            if (!atCreek && !atSouthCreek)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "There's nothing to swing on here.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Determine destination (swing to the other side)
            string destination = atCreek ? "SouthCreek" : "Creek";
            string fromSide = atCreek ? "north" : "south";
            string toSide = atCreek ? "south" : "north";

            // Update location
            _saveGameData.PreviousLocation = _saveGameData.CurrentLocation;
            _saveGameData.CurrentLocation = destination;
            _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddMinutes(2);

            var destLocation = WorldData.Instance.GetLocation(destination);

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: $"You grab the jungle vine hanging over the creek and swing across! " +
                    $"The vine holds firm as you arc over the water, safely landing on the {toSide} side.\n\n" +
                    (destLocation?.Description ?? $"You are at the {toSide} side of the creek."),
                saveGameData: _saveGameData,
                imageFilename: destLocation?.Image,
                locationDescription: destLocation?.Description,
                commandHistory: null));
        }
    }
}
