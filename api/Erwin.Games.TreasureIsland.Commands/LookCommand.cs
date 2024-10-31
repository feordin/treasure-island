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
        public async Task<ProcessCommandResponse?> Execute()
        {
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);
            if (_saveGameData != null)
            {
                _saveGameData.CurrentDateTime += new TimeSpan(0, 1, 0);
            }
            

            return new ProcessCommandResponse(
                "You look around and see: " + (currentLocation != null ? await currentLocation.GetDescription(_saveGameData) : "nothing"), _saveGameData, null, null, null);
        }
    }
}