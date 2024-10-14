using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class LoadGameCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;

        private string? _param;
        public LoadGameCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _param = param;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            if (_param != null)
            {
                if (int.TryParse(_param, out var newGameNumber))
                {
                    var loadedGameData = await _gameDataRepository.LoadGameAsync(ClientPrincipal.Instance?.UserDetails, newGameNumber);
                    if (loadedGameData != null){
                        var commandHistory = await _gameDataRepository.LoadCommandHistoryAsync(ClientPrincipal.Instance?.UserDetails, newGameNumber);
                        var currentLocation = WorldData.Instance?.GetLocation(loadedGameData.CurrentLocation);
                        return new ProcessCommandResponse(
                            "Game " + _param + " loaded.",
                            _saveGameData,
                            currentLocation?.Image,
                            currentLocation?.Description,
                            commandHistory,
                            null);
                    }
                }
            }

            return new ProcessCommandResponse(
                "Error loading game " + _param + ".",
                _saveGameData,
                null,
                null,
                null);
        }
    }
}