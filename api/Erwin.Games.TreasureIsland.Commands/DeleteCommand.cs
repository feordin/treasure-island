using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class DeleteCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;

        private string? _param;
        public DeleteCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _param = param;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            if (_param != null)
            {
                var savedGames = await _gameDataRepository.GetAllSavedGamesAsync(ClientPrincipal.Instance?.UserDetails);

                if (int.TryParse(_param, out var newGameNumber) && newGameNumber > 0
                    && newGameNumber <= savedGames?.Count)
                {
                    var gameId = savedGames.ElementAt(newGameNumber).id;
                    if (gameId != null)
                    {
                        await _gameDataRepository.DeleteGameAsync(gameId);
                        var newSavedGamesList = new List<SaveGameData>(savedGames ?? Enumerable.Empty<SaveGameData>());
                        newSavedGamesList.RemoveAt(newGameNumber);
                        return new ProcessCommandResponse(
                            "Game " + _param + " deleted.",
                            _saveGameData,
                            null,
                            null,
                            null,
                            newSavedGamesList);
                    }
                }
            }

            return new ProcessCommandResponse(
                "Error deleting game " + _param + ".",
                _saveGameData,
                null,
                null,
                null);
        }
    }
}