using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class StartupGameEngineCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        public StartupGameEngineCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            // See if the user has an autosave game
            // If so then load that,
            // otherwise call start new game

            _saveGameData = await _gameDataRepository.LoadGameAsync(ClientPrincipal.Instance?.UserDetails, 0);

            if (_saveGameData != null)
            {
                var currentLocation = WorldData.Instance?.GetLocation(_saveGameData.CurrentLocation);
                return new ProcessCommandResponse(
                    currentLocation?.Description,
                    _saveGameData,
                    currentLocation?.Image);
            }

            if (ClientPrincipal.Instance?.UserDetails != null)
            {
                _saveGameData = await _gameDataRepository.LoadGameAsync("start", 0);
                if (_saveGameData != null)
                    _saveGameData.Player = ClientPrincipal.Instance?.UserDetails;
            }

            return new ProcessCommandResponse(
                WorldData.Instance?.Locations?.FirstOrDefault()?.Description,
                _saveGameData,
                WorldData.Instance?.Locations?.FirstOrDefault()?.Image);
        }
    }
}