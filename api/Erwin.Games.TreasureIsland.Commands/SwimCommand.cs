using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Actions;
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

            var currentLocation = _saveGameData.CurrentLocation;

            // Global drowning check: swimming with King Tut's treasure is fatal
            bool hasKingsTutTreasure = _saveGameData.Inventory?.Any(item =>
                item.Equals("kingsTutTreasure", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (hasKingsTutTreasure && (
                currentLocation?.Equals("FissureRoom", StringComparison.OrdinalIgnoreCase) == true ||
                currentLocation?.Equals("FissureLedge", StringComparison.OrdinalIgnoreCase) == true))
            {
                return HandleTreasureDrowning();
            }

            // Handle swimming at the Creek - DEATH BY CROCODILES
            if (currentLocation?.Equals("Creek", StringComparison.OrdinalIgnoreCase) == true ||
                currentLocation?.Equals("SouthCreek", StringComparison.OrdinalIgnoreCase) == true)
            {
                return HandleCreekSwim();
            }

            // Handle swimming at the Lagoon - Enter the water
            if (currentLocation?.Equals("Lagoon", StringComparison.OrdinalIgnoreCase) == true)
            {
                return HandleLagoonSwim();
            }

            // Handle swimming in FissureRoom
            if (currentLocation?.Equals("FissureRoom", StringComparison.OrdinalIgnoreCase) == true)
            {
                return HandleFissureSwim();
            }

            // Handle swimming from FissureLedge back to FissureRoom
            if (currentLocation?.Equals("FissureLedge", StringComparison.OrdinalIgnoreCase) == true)
            {
                return HandleFissureLedgeSwim();
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "There's nowhere to swim here.",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> HandleCreekSwim()
        {
            // Track previous location for last-chance escape
            _saveGameData!.PreviousLocation = _saveGameData.CurrentLocation;

            var response = new ProcessCommandResponse(
                message: "",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null);

            // Try last-chance escape before death
            if (LastChanceEscape.TryEscape(response, "crocodile attack"))
            {
                return Task.FromResult<ProcessCommandResponse?>(response);
            }

            // Move to crocodile death location
            _saveGameData.CurrentLocation = "CrocodileDeath";

            var deathLocation = WorldData.Instance?.GetLocation("CrocodileDeath");

            _saveGameData.AddEvent("GameOver", "Eaten by crocodiles in the creek", _saveGameData.CurrentDateTime);

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "You dive into the creek, thinking it looks refreshing. But the water erupts with thrashing as massive crocodiles surge toward you! Their powerful jaws snap shut and drag you under. Your adventure ends here in the murky depths...",
                saveGameData: _saveGameData,
                imageFilename: deathLocation?.Image ?? "crocodiledeath.png",
                locationDescription: deathLocation?.Description,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> HandleLagoonSwim()
        {
            // Move to LagoonSwimming
            _saveGameData!.PreviousLocation = _saveGameData.CurrentLocation;
            _saveGameData.CurrentLocation = "LagoonSwimming";
            _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddMinutes(10);

            var lagoonSwimming = WorldData.Instance?.GetLocation("LagoonSwimming");

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "You wade into the crystal-clear lagoon and begin swimming. The water is warm and inviting.\n\n" +
                    (lagoonSwimming?.Description ?? "You are swimming in the lagoon."),
                saveGameData: _saveGameData,
                imageFilename: lagoonSwimming?.Image,
                locationDescription: lagoonSwimming?.Description,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> HandleFissureSwim()
        {
            // Check if the fissure has water (ice was melted)
            if (_saveGameData!.GetEvent("fissure_filled") == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "The fissure is deep and dry. You cannot cross it. Perhaps if it were filled with water...",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Swim across to FissureLedge
            _saveGameData.PreviousLocation = _saveGameData.CurrentLocation;
            _saveGameData.CurrentLocation = "FissureLedge";
            _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddMinutes(5);

            var fissureLedge = WorldData.Instance?.GetLocation("FissureLedge");

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "You dive into the cold water and swim across the fissure. The water is shockingly cold from the melted ice, but you make it across safely to the narrow ledge.\n\n" +
                    (fissureLedge?.Description ?? "You are on a narrow ledge on the far side of the fissure."),
                saveGameData: _saveGameData,
                imageFilename: fissureLedge?.Image,
                locationDescription: fissureLedge?.Description,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> HandleFissureLedgeSwim()
        {
            // Check if the fissure has water (ice was melted)
            if (_saveGameData!.GetEvent("fissure_filled") == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "The fissure is deep and dry. You cannot swim across without water.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Swim back to FissureRoom
            _saveGameData.PreviousLocation = _saveGameData.CurrentLocation;
            _saveGameData.CurrentLocation = "FissureRoom";
            _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime.AddMinutes(5);

            var fissureRoom = WorldData.Instance?.GetLocation("FissureRoom");

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "You dive into the cold water and swim back across the fissure to the main room.\n\n" +
                    (fissureRoom?.Description ?? "You are in the fissure room."),
                saveGameData: _saveGameData,
                imageFilename: fissureRoom?.Image,
                locationDescription: fissureRoom?.Description,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> HandleTreasureDrowning()
        {
            _saveGameData!.PreviousLocation = _saveGameData.CurrentLocation;

            var response = new ProcessCommandResponse(
                message: "",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null);

            // Try last-chance escape before death
            if (LastChanceEscape.TryEscape(response, "drowning with heavy treasure"))
            {
                return Task.FromResult<ProcessCommandResponse?>(response);
            }

            _saveGameData.AddEvent("GameOver", "Drowned while swimming with King Tut's treasure", _saveGameData.CurrentDateTime);
            _saveGameData.CurrentLocation = "GameOver";

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "You plunge into the icy water, but King Tut's golden mask drags you down like an anchor. The massive treasure is far too heavy to swim with. You struggle desperately, but the weight pulls you under the dark water... Your adventure ends here in the flooded fissure.",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }
    }
}
