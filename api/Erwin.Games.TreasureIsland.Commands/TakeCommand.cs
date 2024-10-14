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
            if (currentLocation?.Name == "Bank")
            {
                var bankCommand = new BankCommand(_saveGameData, _gameDataRepository, _command, _param);
                return bankCommand.Execute();
            }
            
            if (_param == null){
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "What do you want to take?",
                    _saveGameData,
                    null,
                    null,
                    null));
            }

            var currentItems = currentLocation?.GetCurrentItems(_saveGameData);

            if (currentItems?.Contains(_param) == true && currentLocation?.Name != null)
            {
                _saveGameData?.Inventory?.Add(_param);
                var locationChange = new LocationChange(currentLocation.Name, _param, false, _saveGameData?.CurrentDateTime);
                
                if (_saveGameData != null && _saveGameData?.LocationChanges == null)
                {
                    _saveGameData.LocationChanges = new List<LocationChange>();
                }
                _saveGameData?.LocationChanges?.Add(locationChange);

                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You take the " + _param + ".",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
            else
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "The " + _param + "is not here.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
        }
    }
}