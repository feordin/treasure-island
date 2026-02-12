using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class PawnCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        private string? _param;
        public PawnCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _command = command;
            _param = param;
        }
        public Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null || WorldData.Instance == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "Unable to get the current game state or world data.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }

            // check the current location, if it's the bank, then we process this as a steal or rob command
            var currentLocation = WorldData.Instance.GetLocation(_saveGameData.CurrentLocation);
            
            if (_param == null){
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "What do you want to pawn?",
                    _saveGameData,
                    null,
                    null,
                    null));
            }

            // Resolve fuzzy item name against inventory
            var resolvedParam = WorldData.Instance?.ResolveItemName(_param, _saveGameData.Inventory) ?? _param;
            var displayName = WorldData.Instance?.GetItemDisplayName(resolvedParam) ?? resolvedParam;
            var currentItems = currentLocation?.GetCurrentItems(_saveGameData);
            var itemDetails = WorldData.Instance?.GetItem(resolvedParam);

            // we need another check here to make sure you have item in your inventory
            if(_saveGameData?.Inventory?.Contains(resolvedParam, StringComparer.OrdinalIgnoreCase) == true &&
                currentLocation?.Name != null)
            {
                _saveGameData?.Inventory?.RemoveAt(_saveGameData.Inventory.FindIndex(n => n.Equals(resolvedParam, StringComparison.OrdinalIgnoreCase)));

                if (resolvedParam.Equals("therepublic", StringComparison.OrdinalIgnoreCase) && _saveGameData != null)
                {
                    currentLocation.AddItemToLocation(_saveGameData, resolvedParam + " pawned");
                    _saveGameData.Money += 5;
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "The owner gives a grunt.  He hands over 5 gold.",
                    _saveGameData,
                    null,
                    null,
                    null));
                }
                else if (resolvedParam.Equals("monkeysPaw", StringComparison.OrdinalIgnoreCase) && _saveGameData != null)
                {
                    _saveGameData.Money += 8;
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "He smiles with delight.  He hands over 8 gold.",
                    _saveGameData,
                    null,
                    null,
                    null));
                }
                else
                {
                    currentLocation.AddItemToLocation(_saveGameData, resolvedParam + " pawned");
                    if (_saveGameData != null)
                        _saveGameData.Money += 1;
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "The owner gives a grunt.  He hands over 1 gold.",
                    _saveGameData,
                    null,
                    null,
                    null));
                }
            }
            else
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You don't have " + displayName + " in your inventory.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
        }
    }
}