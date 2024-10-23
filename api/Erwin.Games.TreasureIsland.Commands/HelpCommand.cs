using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class HelpCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private readonly string basicHelp = "Generally is this game, you can move around by going south, north, east, west, up and down.  You can look and exmaine things, and you can take things.  Sometimes you migth be able to buy or borrow, open or close or climb.  You'll have to experiment a bit with different commands.  Some locations in the game will have additional hints.  See below if this location does.";
        
        public HelpCommand(SaveGameData? saveGameData)
        {
            _saveGameData = saveGameData;
        }
        public Task<ProcessCommandResponse?> Execute()
        {
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            if (currentLocation?.Help != null && currentLocation.Help != "no")
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                     basicHelp + "\n\n" + currentLocation.Help, _saveGameData, null, null, null));
            }
            
            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                basicHelp + "\n\nNo additional hints are available for this location.  You'll have to help yourself for now.", _saveGameData, null, null, null));
        }
    }
}