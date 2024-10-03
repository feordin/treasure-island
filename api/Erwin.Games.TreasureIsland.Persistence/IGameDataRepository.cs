using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Persistence
{
    public interface IGameDataRepository
    {
        Task<SaveGameData?> LoadGameAsync(string? user = null, int gameNumber = 0);
        Task<bool> SaveGameAsync(SaveGameData saveGameData, int gameNumber = 0);
        Task<WorldData?> LoadWorldDataAsync();
        Task<bool> DeleteGameAsync(int gameNumber);
    }
}