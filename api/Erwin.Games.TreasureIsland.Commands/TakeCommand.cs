using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class TakeCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        private string? _param;
        public TakeCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _command = command;
            _param = param;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null || WorldData.Instance == null)
            {
                return new ProcessCommandResponse(
                    "Unable to get the current game state or world data.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            _saveGameData.CurrentDateTime += new TimeSpan(0, 1, 0);

            // check the current location, if it's the bank, then we process this as a steal or rob command
            var currentLocation = WorldData.Instance.GetLocation(_saveGameData.CurrentLocation);
            if (currentLocation?.Name == "Bank")
            {
                var bankCommand = new BankCommand(_saveGameData, _gameDataRepository, _command, _param);
                return await bankCommand.Execute();
            }
            
            if (_param == null){
                return new ProcessCommandResponse(
                    "What do you want to take?",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            // Handle "take all" command
            if (_param.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                return HandleTakeAll(currentLocation);
            }

            var currentItems = currentLocation?.GetCurrentItems(_saveGameData);
            var itemDetails = WorldData.Instance?.GetItem(_param);

            if (itemDetails?.IsMustBuy == true && _saveGameData.Money < itemDetails?.Cost)
            {
                return new ProcessCommandResponse(
                    "You try take the " + _param + " without having enough to money to pay.  Sorry, but the watch have a careful eye!  You stop before you end up in jail.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            if (itemDetails?.IsMustBuy == true && _saveGameData.Money >= itemDetails?.Cost)
            {
                var buyCommand = new BuyCommand(_saveGameData, _gameDataRepository, _command, _param);
                return await buyCommand.Execute();
            }

            // we need another check here to make sure the is actually possible to take
            if (currentItems?.Contains(_param, StringComparer.OrdinalIgnoreCase) == true &&
                currentLocation?.Name != null &&
                (itemDetails == null || itemDetails.IsTakeable == true))
            {
                _saveGameData?.Inventory?.Add(_param);
                // check if the saved game already has any changes to this location
                currentLocation.RemoveItemFromLocation(_saveGameData, _param);

                // Weight warning for King Tut's treasure
                if (_param.Equals("kingsTutTreasure", StringComparison.OrdinalIgnoreCase))
                {
                    return new ProcessCommandResponse(
                        "You take the kingsTutTreasure. This treasure is incredibly heavy... Swimming while carrying it would be nearly impossible.",
                        _saveGameData,
                        null,
                        null,
                        null);
                }

                // Special handling for fakeBook - opens secret passage in Library
                if (_param.Equals("fakeBook", StringComparison.OrdinalIgnoreCase) || _param.Equals("fakebook", StringComparison.OrdinalIgnoreCase))
                {
                    _saveGameData.AddEvent("library_secret_opened", "The secret bookshelf passage has been opened", _saveGameData.CurrentDateTime);

                    // Add west movement to Library
                    var library = WorldData.Instance?.GetLocation("Library");
                    if (library != null)
                    {
                        var westMovement = new Movement
                        {
                            Direction = new[] { "west" },
                            Destination = "HiddenRoom",
                            TimeToMove = 1
                        };
                        library.AddMovementToLocation(_saveGameData, westMovement);
                    }

                    return new ProcessCommandResponse(
                        "As you pull the fake book, the bookshelf swings open revealing a hidden room to the west!",
                        _saveGameData,
                        null,
                        null,
                        null);
                }

                return new ProcessCommandResponse(
                    "You take the " + _param + ".",
                    _saveGameData,
                    null,
                    null,
                    null);
            }
            else
            {
                return new ProcessCommandResponse(
                    "The " + _param + " is not here, or you can't take it.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }
        }

        private ProcessCommandResponse HandleTakeAll(Location? currentLocation)
        {
            var currentItems = currentLocation?.GetCurrentItems(_saveGameData);

            if (currentItems == null || currentItems.Count == 0)
            {
                return new ProcessCommandResponse(
                    "There is nothing here to take.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            var takenItems = new List<string>();
            var itemsToTake = currentItems.ToList();

            foreach (var item in itemsToTake)
            {
                var itemDetails = WorldData.Instance?.GetItem(item);

                // Skip items that can't be taken or must be bought
                if (itemDetails?.IsTakeable == false || itemDetails?.IsMustBuy == true)
                {
                    continue;
                }

                _saveGameData?.Inventory?.Add(item);
                currentLocation?.RemoveItemFromLocation(_saveGameData, item);
                takenItems.Add(item);
            }

            if (takenItems.Count == 0)
            {
                return new ProcessCommandResponse(
                    "There is nothing here you can take.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            return new ProcessCommandResponse(
                $"You take: {string.Join(", ", takenItems)}.",
                _saveGameData,
                null,
                null,
                null);
        }
    }
}