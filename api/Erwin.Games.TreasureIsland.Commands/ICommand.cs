using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public interface ICommand
    {
        Task<ProcessCommandResponse?> Execute();
    }
}