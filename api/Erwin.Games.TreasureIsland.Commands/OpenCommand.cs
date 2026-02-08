using System;
using System.Linq;
using System.Text.RegularExpressions;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class OpenCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly IGameDataRepository _repository;
        private readonly string? _objectName;
        private readonly string? _combination;

        private static readonly int[] CorrectCombination = { 7, 23, 42 };

        public OpenCommand(SaveGameData? saveGameData, IGameDataRepository repository, string? objectName, string? combination)
        {
            _saveGameData = saveGameData;
            _repository = repository;
            _objectName = objectName;
            _combination = combination;
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

            _saveGameData.CurrentDateTime += new TimeSpan(0, 1, 0);

            if (string.IsNullOrEmpty(_objectName) ||
                !_objectName.Equals("safe", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "What do you want to open?",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Must be at SittingRoom
            if (_saveGameData.CurrentLocation?.Equals("SittingRoom", StringComparison.OrdinalIgnoreCase) != true)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "There is no safe here.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Already opened
            if (_saveGameData.GetEvent("safe_opened") != null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "The safe is already open.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Need combination
            if (string.IsNullOrEmpty(_combination))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You need to provide the combination to open the safe.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Parse combination numbers from any format (7-23-42, 7,23,42, 7 23 42, etc.)
            var matches = Regex.Matches(_combination, @"\d+");
            var numbers = matches.Select(m => int.Parse(m.Value)).ToArray();

            if (numbers.SequenceEqual(CorrectCombination))
            {
                _saveGameData.AddEvent("safe_opened", "Opened the safe with combination 7-23-42", _saveGameData.CurrentDateTime);

                // Add bundleOfBills to SittingRoom
                var sittingRoom = WorldData.Instance.GetLocation("SittingRoom");
                sittingRoom?.AddItemToLocation(_saveGameData, "bundleOfBills");

                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You carefully turn the dial... 7... 23... 42... Click! The safe swings open, revealing a bundle of bills inside!",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "The combination doesn't work. The safe remains locked.",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }
    }
}
