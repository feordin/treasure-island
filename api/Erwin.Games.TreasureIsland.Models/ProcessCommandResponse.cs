using Erwin.Games.TreasureIsland.Models;
using System.Text.Json.Serialization;
namespace Erwin.Games.TreasureIsland.Models
{
    public record ProcessCommandResponse(string? Message,
            SaveGameData? saveGameData,
            string? imageFilename,
            string? locationDescription,
            CommandHistory? commandHistory,
            [property: JsonIgnore] List<SaveGameData>? savedGames = null)
    {
        public List<SaveGameData> SavedGames { get; init; } = savedGames ?? new List<SaveGameData>();
    }
}
