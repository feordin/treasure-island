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
                var history = await _gameDataRepository.LoadCommandHistoryAsync(ClientPrincipal.Instance?.UserDetails, 0);
                var currentLocation = WorldData.Instance?.GetLocation(_saveGameData.CurrentLocation);
                List<SaveGameData>? savedGamesList = null;
                var savedGames = await _gameDataRepository.GetAllSavedGamesAsync(ClientPrincipal.Instance?.UserDetails);
                if (savedGames != null)
                {
                    savedGamesList = savedGames.ToList();
                }
                
                return new ProcessCommandResponse(
                    currentLocation?.Description,
                    _saveGameData,
                    currentLocation?.Image,
                    currentLocation?.Description,
                    history,
                    savedGamesList);
            }

            if (ClientPrincipal.Instance?.UserDetails != null)
            {
                _saveGameData = await _gameDataRepository.LoadGameAsync("start", 0);
                if (_saveGameData != null)
                {
                    System.Diagnostics.Debug.WriteLine("Current identified user is: " + ClientPrincipal.Instance?.UserDetails);
                    _saveGameData.Player = ClientPrincipal.Instance?.UserDetails;
                }
            }

            return new ProcessCommandResponse(
                WorldData.Instance?.IntroText + "\n\n" + WorldData.Instance?.Locations?.FirstOrDefault()?.Description,
                _saveGameData,
                WorldData.Instance?.Locations?.FirstOrDefault()?.Image,
                WorldData.Instance?.Locations?.FirstOrDefault()?.Description,
                null);
        }
    }
}