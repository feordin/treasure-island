using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class SaveGameCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;

        private string? _param;
        public SaveGameCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _param = param;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null)
            {
                return new ProcessCommandResponse(
                    "Error I didn't have any game data to save.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            var savedGames = await _gameDataRepository.GetAllSavedGamesAsync(ClientPrincipal.Instance?.UserDetails);
            var history = await _gameDataRepository.LoadCommandHistoryAsync(ClientPrincipal.Instance?.UserDetails, 0);

            if (_param == null)
            {
                // assume a new game to save
                // first get the list of saved games
                var newGameNumber = savedGames?.Count ?? 0;
                if (await _gameDataRepository.SaveGameAsync(_saveGameData, newGameNumber) &&
                    await _gameDataRepository.SaveCommandHistoryAsync(history, null, newGameNumber))
                {
                    var newSavedGamesList = new List<SaveGameData>(savedGames ?? Enumerable.Empty<SaveGameData>());
                    newSavedGamesList.Add(_saveGameData);
                    return new ProcessCommandResponse(
                        "Game " + _param + " saved.",
                        _saveGameData,
                        null,
                        null,
                        null,
                        newSavedGamesList);
                }
                else
                {
                    return new ProcessCommandResponse(
                        "Error saving game " + newGameNumber + ".",
                        _saveGameData,
                        null,
                        null,
                        null);
                }
            }
            else
            {
                if (int.TryParse(_param, out var gameNumber))
                {
                    if (gameNumber <= 0 || gameNumber >= savedGames?.Count)
                    {
                        gameNumber = savedGames?.Count ?? 0;
                    }

                    var gameId = savedGames?.ElementAtOrDefault(gameNumber)?.id;
                    if (gameId != null)
                    {
                        var gameIdTokens = gameId.Split('_');
                        var commandHistoryId = gameIdTokens[0] + "_history_" + gameIdTokens[1];

                        if (await _gameDataRepository.SaveGameAsync(_saveGameData, gameId) &&
                            await _gameDataRepository.SaveCommandHistoryAsync(history, commandHistoryId))
                        {
                            var newSavedGamesList = new List<SaveGameData>(savedGames ?? Enumerable.Empty<SaveGameData>());
                            newSavedGamesList.Add(_saveGameData);
                            return new ProcessCommandResponse(
                                "Game " + _param + " saved.",
                                _saveGameData,
                                null,
                                null,
                                null,
                                newSavedGamesList);
                        }
                    }

                    // assume a new game to save
                    // first get the list of saved games
                    var newGameNumber = savedGames?.Count ?? 0;
                    if (await _gameDataRepository.SaveGameAsync(_saveGameData, newGameNumber) &&
                        await _gameDataRepository.SaveCommandHistoryAsync(history, null, newGameNumber))
                    {
                        var newSavedGamesList = new List<SaveGameData>(savedGames ?? Enumerable.Empty<SaveGameData>());
                        newSavedGamesList.Add(_saveGameData);
                        return new ProcessCommandResponse(
                            "Game " + _param + " saved.",
                            _saveGameData,
                            null,
                            null,
                            null,
                            newSavedGamesList);
                    }

                    return new ProcessCommandResponse(
                    "Error saving game " + _param + ".",
                    _saveGameData,
                    null,
                    null,
                    null);

                }
                else
                {
                    return new ProcessCommandResponse(
                        _param + " is not a valid game number.",
                        _saveGameData,
                        null,
                        null,
                        null);
                }
            }
        }
    }
}