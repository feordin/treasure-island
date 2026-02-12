using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    internal class ExamineCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        private string? _param;
        public ExamineCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _command = command;
            _param = param;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            if (string.IsNullOrEmpty(_param))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("What would you like to examine?", _saveGameData, null, null, null));
            }

            // First, find the matching item at current location or inventory using fuzzy resolution
            var itemsAtLocation = currentLocation?.GetCurrentItems(_saveGameData);
            var matchingItemName = WorldData.Instance?.ResolveItemName(_param, itemsAtLocation);

            // Check if resolution actually found a match in location items
            if (matchingItemName != null && itemsAtLocation?.Contains(matchingItemName, StringComparer.OrdinalIgnoreCase) != true)
            {
                // Try inventory
                var inventoryMatch = WorldData.Instance?.ResolveItemName(_param, _saveGameData?.Inventory);
                if (inventoryMatch != null && _saveGameData?.Inventory?.Contains(inventoryMatch, StringComparer.OrdinalIgnoreCase) == true)
                {
                    matchingItemName = inventoryMatch;
                }
                else
                {
                    // Fall back to original substring matching for special cases
                    matchingItemName = itemsAtLocation?.FirstOrDefault(i =>
                        i.Equals(_param, StringComparison.OrdinalIgnoreCase) ||
                        i.Contains(_param, StringComparison.OrdinalIgnoreCase));

                    if (matchingItemName == null)
                    {
                        matchingItemName = _saveGameData?.Inventory?.FirstOrDefault(i =>
                            i.Equals(_param, StringComparison.OrdinalIgnoreCase) ||
                            i.Contains(_param, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            // Get the actual item using the matched name (or fall back to param for special cases)
            var itemNameToUse = matchingItemName ?? _param;
            var item = WorldData.Instance?.GetItem(itemNameToUse);

            // Special case: bushes (adds wallet to alley)
            if (_param == "bushes" && _saveGameData != null)
            {
                var walletEvent = _saveGameData.GetEvent("wallet");
                if (walletEvent == null)
                {
                    currentLocation?.AddItemToLocation(_saveGameData, "wallet");
                    _saveGameData.AddEvent("wallet", "You take a look at the bushes and find a wallet.", _saveGameData.CurrentDateTime);
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("You take a look at the bushes and find a wallet.", _saveGameData, null, null, null));
                }
            }
            // Special case: bookshelf in TrashPit (adds TheRepublic)
            else if (_param == "bookshelf" && _saveGameData != null)
            {
                if (currentLocation?.Name?.Contains("TrashPit", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var bookevent = _saveGameData.GetEvent("book");
                    if (bookevent == null)
                    {
                        currentLocation?.AddItemToLocation(_saveGameData, "TheRepublic");
                        _saveGameData.AddEvent("book", "Amazingly, wrapped in oil cloth, an old and valuable book of greek philosophy has survived.  This might be something the bank would consider collateral for a loan.", _saveGameData.CurrentDateTime);
                        return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("Amazingly, wrapped in oil cloth, an old and valuable book of greek philosophy has survived.  This might be something the bank would consider collateral for a loan.", _saveGameData, null, null, null));
                    }
                }
            }
            // Special case: sand
            else if (_param.Contains("sand", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (currentLocation?.Name?.Contains("GiantFootprint", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("You take a closer look at that sandy patch ahead, that is definitely quicksand!  Better stay away.", _saveGameData, null, null, null));
                }
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("You take a look at the sand.  It's just sand.", _saveGameData, null, null, null));
            }
            // Special case: GoblinTower
            else if (_param.Contains("GoblinTower", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (currentLocation?.Name?.Contains("GoblinValley", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // Check if flashlight is on
                    if (_saveGameData?.GetEvent("flashlight_on") == null)
                    {
                        return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                            "It's too dark to see anything behind the goblin tower. You need a light source.",
                            _saveGameData, null, null, null));
                    }

                    var diamondsEvent = _saveGameData?.GetEvent("diamonds");
                    if (diamondsEvent == null)
                    {
                        currentLocation?.AddItemToLocation(_saveGameData, "diamonds");
                        _saveGameData?.AddEvent("diamonds", "Found the diamonds behind the goblin tower.", _saveGameData.CurrentDateTime);
                        return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                            "You shine your flashlight behind the goblin tower. Even covered in dust, it is unmistakable - diamonds in the rough!",
                            _saveGameData, null, null, null));
                    }
                }
            }

            // Generic Reveals handling - check if the matched item reveals something
            if (item?.Reveals != null && matchingItemName != null)
            {
                var itemEvent = _saveGameData?.GetEvent(matchingItemName);
                if (itemEvent == null)
                {
                    var revealedDisplayName = WorldData.Instance?.GetItemDisplayName(item.Reveals) ?? item.Reveals;
                    var matchDisplayName = item.GetDisplayName();
                    currentLocation?.AddItemToLocation(_saveGameData, item.Reveals);
                    _saveGameData?.AddEvent(matchingItemName, "You take a look at the " + matchDisplayName + " and find: " + revealedDisplayName + ".", _saveGameData.CurrentDateTime);
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("You take a look at the " + matchDisplayName + " and find: " + revealedDisplayName + ".", _saveGameData, null, null, null));
                }
            }

            // Check if item exists at location or inventory
            if (item == null && matchingItemName == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(
                    new ProcessCommandResponse("There is no " + _param + " here to examine.", _saveGameData, null, null, null));
            }

            var displayName = item?.GetDisplayName() ?? matchingItemName ?? _param;
            var examineText = !string.IsNullOrEmpty(item?.ExamineText)
                ? item.ExamineText
                : item?.Description;

            return Task.FromResult<ProcessCommandResponse?>(
                new ProcessCommandResponse("You take a look at the " + displayName + ".\n\n" + examineText, _saveGameData, null, null, null));
        }
    }
}