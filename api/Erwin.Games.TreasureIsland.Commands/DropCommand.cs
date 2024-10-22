using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class DropCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        private string? _param;
        public DropCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
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
                    "What do you want to drop?",
                    _saveGameData,
                    null,
                    null,
                    null));
            }

            var currentItems = currentLocation?.GetCurrentItems(_saveGameData);
            var itemDetails = WorldData.Instance?.GetItem(_param);

            // we need another check here to make sure the is actually possible to take
            if(_saveGameData?.Inventory?.Contains(_param, StringComparer.OrdinalIgnoreCase) == false && 
                currentLocation?.Name != null)
            {
                _saveGameData?.Inventory?.RemoveAt(_saveGameData.Inventory.FindIndex(n => n.Equals(_param, StringComparison.OrdinalIgnoreCase)));
                // check if the saved game already has any changes to this location
                currentLocation.RemoveItemFromLocation(_saveGameData, _param);
                
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You drop the " + _param + ".",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
            else
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "The " + _param + " is not here, or you can't take it.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
        }
    }
}