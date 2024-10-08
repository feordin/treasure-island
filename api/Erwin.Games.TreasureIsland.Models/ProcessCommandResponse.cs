using Erwin.Games.TreasureIsland.Models;
namespace Erwin.Games.TreasureIsland.Models
{
    public record ProcessCommandResponse(string? Message,
        SaveGameData? saveGameData,
        string? imageFilename,
        string? locationDescription,
        IReadOnlyCollection<SaveGameData>? savedGames);
}
