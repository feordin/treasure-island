using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class SwimCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly IGameDataRepository _repository;
        private readonly string? _direction;

        public SwimCommand(SaveGameData? saveGameData, IGameDataRepository repository, string? direction)
        {
            _saveGameData = saveGameData;
            _repository = repository;
            _direction = direction;
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

            // Check if we're in FissureRoom
            if (!_saveGameData.CurrentLocation?.Equals("FissureRoom", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "There's nothing to swim across here.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if the fissure has water (ice was melted)
            if (_saveGameData.GetEvent("fissure_filled") == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "The fissure is deep and dry. You cannot cross it. Perhaps if it were filled with water...",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Swim across to WestFissureRoom
            _saveGameData.CurrentLocation = "WestFissureRoom";
            _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddMinutes(5);

            var westFissureRoom = WorldData.Instance.Locations?.FirstOrDefault(l =>
                l.Name?.Equals("WestFissureRoom", StringComparison.OrdinalIgnoreCase) == true);

            string locationDesc = westFissureRoom?.Description ?? "You are in the west fissure room.";

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "You dive into the cold water and swim across the fissure. The water is shockingly cold from the melted ice, but you make it across safely.",
                saveGameData: _saveGameData,
                imageFilename: westFissureRoom?.Image,
                locationDescription: locationDesc,
                commandHistory: null));
        }
    }
}
