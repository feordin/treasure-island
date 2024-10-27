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
        private string? _direction;
        public MoveCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string direction, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _direction = direction;
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

            _direction = Location.SimpleToCardinalDirection(_saveGameData.Facing, _direction);

            // check the current location
            var currentLocation = WorldData.Instance.GetLocation(_saveGameData?.CurrentLocation);
            var movement = currentLocation?.AllowedMovements?.FirstOrDefault(m => m.Direction?.Contains(_direction) == true);
            var newLocation = WorldData.Instance.GetLocation(movement?.Destination);

            _saveGameData.Facing = _direction;

            if (movement != null &&  newLocation != null)
            {
                _saveGameData.CurrentLocation = movement.Destination;
                if (movement.TimeToMove != null)
                    _saveGameData.CurrentDateTime = _saveGameData.CurrentDateTime + new TimeSpan(0, movement.TimeToMove.Value, 0);

                return new ProcessCommandResponse(
                    "You go " + _direction + ".\n\n" + (newLocation != null ? await newLocation.GetDescription(_saveGameData) : string.Empty),
                    _saveGameData,
                    newLocation?.Image,
                    newLocation?.Description,
                    null);
            }
            else
            {
                return new ProcessCommandResponse(
                    "You try to go " + _direction + ", " + "but can't and end up in the same place.\n\n" + (currentLocation != null ? await currentLocation.GetDescription(_saveGameData) : string.Empty),
                    _saveGameData,
                    newLocation?.Image,
                    newLocation?.Description,
                    null);
            }
        }
    }
}