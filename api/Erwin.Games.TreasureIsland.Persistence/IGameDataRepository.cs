using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Persistence
{
    public interface IGameDataRepository
    {
        Task<SaveGameData?> LoadGameAsync(string? user = null, int gameNumber = 0);
        Task<SaveGameData?> LoadGameAsync(string gameId);
        Task<bool> SaveGameAsync(SaveGameData? saveGameData, int gameNumber = 0);
        Task<bool> SaveGameAsync(SaveGameData? saveGameData, string gameId);
        Task<WorldData?> LoadWorldDataAsync();
        Task<bool> DeleteGameAsync(int gameNumber);
        Task<bool> DeleteGameAsync(string gameId);
        Task<CommandHistory?> LoadCommandHistoryAsync(string? user, int gameNumber);
        Task<CommandHistory?> LoadCommandHistoryAsync(string historyId);
        Task<bool> SaveCommandHistoryAsync(CommandHistory? commandHistory, string? user, int gameNumber);
        Task<bool> SaveCommandHistoryAsync(CommandHistory? commandHistory, string historyId);
        Task<IReadOnlyCollection<SaveGameData>?> GetAllSavedGamesAsync(string? user);
        Task<bool> AddToGameHistory(string? command, string? response, string? user, int gameNumber);
    }
}