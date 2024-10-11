using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class MoveCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _direction;
        public MoveCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string direction, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _direction = direction;
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

            // check the current location
            var currentLocation = WorldData.Instance.GetLocation(_saveGameData.CurrentLocation);
            var movement = currentLocation?.AllowedMovements?.FirstOrDefault(m => m.Direction?.Contains(_direction) == true);
            var newLocation = WorldData.Instance.GetLocation(movement?.Destination);

            if (movement != null &&  newLocation != null)
            {
                _saveGameData.CurrentLocation = movement.Destination;
                if (movement.TimeToMove != null)
                    _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime + new TimeSpan(0, movement.TimeToMove.Value, 0);

                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You go " + _direction + ".\n\n" + newLocation?.Description,
                    _saveGameData,
                    newLocation?.Image,
                    newLocation?.Description,
                    null));
            }
            else
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You try to go " + _direction + ".\n\n" + "but can't and end up in the same place.",
                    _saveGameData,
                    newLocation?.Image,
                    newLocation?.Description,
                    null));
            }
        }
    }
}