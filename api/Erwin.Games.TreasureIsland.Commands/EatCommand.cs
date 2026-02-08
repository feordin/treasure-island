using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Actions;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class EatCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly IGameDataRepository _repository;
        private readonly string? _target;

        public EatCommand(SaveGameData? saveGameData, IGameDataRepository repository, string? target)
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

            if (string.IsNullOrWhiteSpace(_target))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "What do you want to eat?",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check for mushrooms (eat in-place at MushroomRoom)
            if (_target.Equals("mushrooms", StringComparison.OrdinalIgnoreCase))
            {
                return HandleEatMushrooms();
            }

            // Check for donuts (two-bite mechanic)
            if (_target.Equals("donuts", StringComparison.OrdinalIgnoreCase))
            {
                return HandleEatDonuts();
            }

            // Check for other food items in inventory
            return HandleEatGenericItem();
        }

        private Task<ProcessCommandResponse?> HandleEatMushrooms()
        {
            // Mushrooms are Takeable: false, so they can only be eaten at the location
            var currentLocation = _saveGameData!.CurrentLocation;

            if (!currentLocation?.Equals("MushroomRoom", StringComparison.OrdinalIgnoreCase) == true)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "There's nothing like that to eat here.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check mushrooms are still at the location
            var location = WorldData.Instance?.GetLocation("MushroomRoom");
            var currentItems = location?.GetCurrentItems(_saveGameData);
            if (currentItems == null || !currentItems.Contains("mushrooms"))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "There's nothing like that to eat here.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Set previous location for last-chance escape
            _saveGameData.PreviousLocation = _saveGameData.CurrentLocation;

            // Add the ate_mushrooms event
            _saveGameData.AddEvent("ate_mushrooms", "Ate poisonous mushrooms in the mushroom room", _saveGameData.CurrentDateTime);

            var response = new ProcessCommandResponse(
                message: "",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null);

            // Try last-chance escape before death
            if (LastChanceEscape.TryEscape(response, "mushroom poisoning"))
            {
                return Task.FromResult<ProcessCommandResponse?>(response);
            }

            // Death by mushroom poisoning
            _saveGameData.AddEvent("GameOver", "Poisoned by eating mushrooms in the mushroom room", _saveGameData.CurrentDateTime);
            _saveGameData.CurrentLocation = "GameOver";

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: "You grab a handful of the colorful mushrooms and pop them in your mouth. They taste surprisingly sweet at first, but then a burning sensation spreads through your body. The cavern begins to spin as the deadly poison takes hold. You collapse to the ground as everything fades to black... Your adventure ends here.",
                saveGameData: _saveGameData,
                imageFilename: location?.Image ?? "mushroomroom.png",
                locationDescription: null,
                commandHistory: null));
        }

        private Task<ProcessCommandResponse?> HandleEatDonuts()
        {
            // Donuts must be in inventory
            bool hasDonuts = _saveGameData!.Inventory?.Any(item =>
                item.Equals("donuts", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasDonuts)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You don't have that.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if this is the first or second bite
            if (_saveGameData.GetEvent("ate_donuts") == null)
            {
                // First bite - add event but keep donuts
                _saveGameData.AddEvent("ate_donuts", "Ate some of the donuts", _saveGameData.CurrentDateTime);

                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You eat some of the donuts. They're delicious! You still have some left - better save them though, they might come in handy.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }
            else
            {
                // Second bite - remove donuts from inventory
                _saveGameData.Inventory?.RemoveAll(item =>
                    item.Equals("donuts", StringComparison.OrdinalIgnoreCase));

                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You eat the rest of the donuts. They're all gone now.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }
        }

        private Task<ProcessCommandResponse?> HandleEatGenericItem()
        {
            // Check if item is in inventory
            bool hasItem = _saveGameData!.Inventory?.Any(item =>
                item.Equals(_target, StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasItem)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You don't have that.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Remove item from inventory
            _saveGameData.Inventory?.RemoveAll(item =>
                item.Equals(_target, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message: $"You eat the {_target}. Not bad!",
                saveGameData: _saveGameData,
                imageFilename: null,
                locationDescription: null,
                commandHistory: null));
        }
    }
}
