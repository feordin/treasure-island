using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public static class CommandFactory
    {
        public static ICommand CreateCommand(string? commandName, SaveGameData? saveGameData, IGameDataRepository repository)
        {
            commandName = commandName?.Trim().ToLower();
            var commandTokens = commandName?.Split(' ');
            commandName = commandTokens?[0];
            var commandParam = commandTokens?.Length > 1 ? commandTokens[1] : null;

            switch (commandName)
            {
                case "startup":
                    return new StartupGameEngineCommand(saveGameData, repository);
                case "new":
                    return new StartNewGame(saveGameData, repository);
                case "look":
                    return new LookCommand(saveGameData);
                case "north":
                case "south":
                case "east":
                case "west":
                case "up":
                case "down":
                    return new MoveCommand(saveGameData, repository, commandName, commandParam);
                default:
                    return new UnknownCommand(saveGameData, repository);
            }
        }
    }
}