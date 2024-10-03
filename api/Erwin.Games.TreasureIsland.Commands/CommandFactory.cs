using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public static class CommandFactory
    {
        public static ICommand CreateCommand(string? commandName, SaveGameData? saveGameData, IGameDataRepository repository)
        {
            switch (commandName)
            {
                case "init":
                    return new InitializeCommand(saveGameData, repository);
                default:
                    return new UnknownCommand(saveGameData, repository);
            }
        }
    }
}