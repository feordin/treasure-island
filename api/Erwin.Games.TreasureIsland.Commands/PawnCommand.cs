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
                currentLocation.AddItemToLocation(_saveGameData, _param + " pawned");

                if (_param == "therepublic" && _saveGameData != null)
                {
                    _saveGameData.Money += 5;
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "The owner gives a grunt.  He hands over 5 gold.",
                    _saveGameData,
                    null,
                    null,
                    null));
                }
                else
                {
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
                    "You don't have a " + _param + " in your inventory.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
        }
    }
}