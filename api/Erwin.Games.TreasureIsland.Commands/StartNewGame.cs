using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class StartNewGame : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        public StartNewGame(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            _saveGameData = await _gameDataRepository.LoadGameAsync("start", 0);
            
            if (_saveGameData != null)
                _saveGameData.Player = ClientPrincipal.Instance?.UserDetails;

            var currentLocation = WorldData.Instance?.Locations?.FirstOrDefault();

            return new ProcessCommandResponse(
                currentLocation?.Description,
                _saveGameData,
                currentLocation?.Image,
                currentLocation?.Description,
                null);
        }
    }
}