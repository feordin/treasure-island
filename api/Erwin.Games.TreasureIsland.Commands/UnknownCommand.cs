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
        private string? _param;
        public UnknownCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _param = param;
        }
        public Task<ProcessCommandResponse?> Execute()
        {
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);
            return Task.FromResult<ProcessCommandResponse?>(
                new ProcessCommandResponse(
                    "I am afraid I don't understand that command.\n\n" + _param,
                    _saveGameData,
                    currentLocation?.Image,
                    currentLocation?.Description,
                    null));
        }
    }
}