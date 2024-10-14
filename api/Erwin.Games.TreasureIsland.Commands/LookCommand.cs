using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class LookCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        
        public LookCommand(SaveGameData? saveGameData)
        {
            _saveGameData = saveGameData;
        }
        public Task<ProcessCommandResponse?> Execute()
        {
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            return Task.FromResult<ProcessCommandResponse?>(
                new ProcessCommandResponse("You look around and see: " + currentLocation?.Description, _saveGameData,null, null, null));
        }
    }
}