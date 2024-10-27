using Erwin.Games.TreasureIsland.Models;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Erwin.Games.TreasureIsland.Models
{
    public record ProcessCommandResponse(
        [property: JsonIgnore] string message,
        SaveGameData? saveGameData,
        string? imageFilename,
        string? locationDescription,
        CommandHistory? commandHistory,
        [property: JsonIgnore] List<SaveGameData>? savedGames = null)
    {
        public string? Message { get; set; } = message ?? string.Empty;
        public List<SaveGameData> SavedGames { get; init; } = savedGames ?? new List<SaveGameData>();
    }
}