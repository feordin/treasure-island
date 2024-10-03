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
        public async Task<ProcessCommandResponse?> Execute()
        {
            var saveGameData = await _gameDataRepository.LoadGameAsync("start", 0);
            WorldData.Instance = await _gameDataRepository.LoadWorldDataAsync();
            return new ProcessCommandResponse("I am afraid I don't understand that command.", _saveGameData, null);
        }
    }
}