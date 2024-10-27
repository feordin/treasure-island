using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime;

namespace Erwin.Games.TreasureIsland.Commands
{
    public static class CommandFactory
    {
        public static ICommand CreateCommand(string? commandName, SaveGameData? saveGameData, IGameDataRepository repository)
        {

            commandName = commandName?.Replace("\"", "").Replace("\\", "").Trim();
            var commandTokens = commandName?.Split(new[] { ' ', '\n', '.' }, StringSplitOptions.RemoveEmptyEntries);
            commandName = commandTokens?[0];
            var commandParam = commandTokens?.Length > 1 ? commandTokens[1] : null;
            commandParam = commandParam?.ToLower();
            var commandRemainder = commandName?.Replace("unknown_command", "");

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
                case "left":
                case "right":
                case "ahead":
                case "behind":
                    return new MoveCommand(saveGameData, repository, commandName, commandParam);
                case "save":
                    return new SaveGameCommand(saveGameData, repository, commandParam);
                case "load":
                    return new LoadGameCommand(saveGameData, repository, commandParam);
                case "delete":
                    return new DeleteCommand(saveGameData, repository, commandParam);
                case "steal":
                case "borrow":
                    return new BankCommand(saveGameData, repository, commandName, commandParam);
                case "take":
                    return new TakeCommand(saveGameData, repository, commandName, commandParam);
                case "embellish":
                    return new EmbellishCommand(saveGameData);
                case "read":
                case "examine":
                    return new ExamineCommand(saveGameData, repository, commandName, commandParam);
                case "drop":
                    return new DropCommand(saveGameData, repository, commandName, commandParam);
                case "buy":
                    return new BuyCommand(saveGameData, repository, commandName, commandParam);
                case "help":
                    return new HelpCommand(saveGameData);
                case "pawn":
                    return new PawnCommand(saveGameData, repository, commandName, commandParam);
                case "pray":
                    return new PrayCommand(saveGameData);
                case "fortune":
                    return new FortuneCommand(saveGameData);
                default:
                    return new UnknownCommand(saveGameData, repository, commandRemainder);
            }
        }
    }
}