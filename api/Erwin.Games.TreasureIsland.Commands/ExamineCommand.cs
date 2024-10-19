using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    internal class ExamineCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        private string? _param;
        public ExamineCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _command = command;
            _param = param;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            return Task.FromResult<ProcessCommandResponse?>(
                new ProcessCommandResponse("You take a look at the " + _param + ".\n\n" + WorldData.Instance?.GetItem(_param)?.Description,_saveGameData, null, null, null));
        }
    }
}