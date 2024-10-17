using System.Threading.Tasks;

namespace Erwin.Games.TreasureIsland.Commands
{
    public interface IAIClient
    {
        Task<string?> ParsePlayerInput(string? input);
        Task<string?> GetEmbelleshedLocationDescription(string? description);
    }
}