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

            if (currentLocation?.Name == "PawnShop")
            {
                var pawnCommand = new PawnCommand(_saveGameData, _gameDataRepository, _command, _param);
                return pawnCommand.Execute();
            }

            var currentItems = currentLocation?.GetCurrentItems(_saveGameData);
            var itemDetails = WorldData.Instance?.GetItem(_param);

            if(_saveGameData?.Inventory?.Contains(_param, StringComparer.OrdinalIgnoreCase) == true && 
                currentLocation?.Name != null)
            {
                _saveGameData?.Inventory?.RemoveAt(_saveGameData.Inventory.FindIndex(n => n.Equals(_param, StringComparison.OrdinalIgnoreCase)));

                if (_param == "wallet" && _saveGameData != null)
                {
                    _saveGameData.Money += 10;
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "The constable looks surprised, but praises you for your honesty.  He begrudgingly hands over the reward of 10 gold.",
                    _saveGameData,
                    null,
                    null,
                    null));
                }

                currentLocation.AddItemToLocation(_saveGameData, _param);
                
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
                    "You don't have a " + _param + " in your inventory.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
        }
    }
}