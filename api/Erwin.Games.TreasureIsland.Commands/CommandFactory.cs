using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public static class CommandFactory
    {
        public static ICommand CreateCommand(string? commandName, SaveGameData? saveGameData, IGameDataRepository repository)
        {
            commandName = commandName?.Trim().ToLower();

            switch (commandName)
            {
                case "startup":
                    return new StartupGameEngineCommand(saveGameData, repository);
                case "new":
                    return new StartNewGame(saveGameData, repository);
                case "look":
                    return new LookCommand(saveGameData);
                default:
                    return new UnknownCommand(saveGameData, repository);
            }
        }
    }
}