using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class UnknownCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        public UnknownCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
        }
        public Task<ProcessCommandResponse?> Execute()
        {
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);
            return Task.FromResult<ProcessCommandResponse?>(
                new ProcessCommandResponse(
                    "I am afraid I don't understand that command.",
                    _saveGameData,
                    currentLocation?.Image,
                    currentLocation?.Description,
                    null));
        }
    }
}