using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class InitializeCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        public InitializeCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            _saveGameData = await _gameDataRepository.LoadGameAsync("start", 0);
            WorldData.Instance = await _gameDataRepository.LoadWorldDataAsync();
            _saveGameData.Player = ClientPrincipal.Instance?.UserDetails;
            return new ProcessCommandResponse(
                WorldData.Instance?.Locations?.FirstOrDefault()?.Description,
                _saveGameData,
                WorldData.Instance?.Locations?.FirstOrDefault()?.Image);
        }
    }
}