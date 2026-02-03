using System.Threading.Tasks;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public interface IAIClient
    {
        Task<string?> ParsePlayerInput(string? input);
        Task<string?> ParsePlayerInputWithAgent(string? input);
        Task<string?> ParsePlayerInputWithAgent(string? input, Location? currentLocation, SaveGameData? saveGame);
        Task<string?> GetEmbelleshedLocationDescription(string? description);
        Task<string?> GetFortune();

        Task<string?> Pray();
    }
}