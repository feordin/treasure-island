using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class EmbellishCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        
        public EmbellishCommand(SaveGameData? saveGameData)
        {
            _saveGameData = saveGameData;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null)
            {
                return new ProcessCommandResponse("Save game data unavailable, cannot change embellishment setting.", _saveGameData, null, null, null);
            }

            if (_saveGameData.AiEmbelleshedDescriptions)
            {
                _saveGameData.AiEmbelleshedDescriptions = false;
                return new ProcessCommandResponse("AI embellishments are off.", _saveGameData, null, null, null);
            }
            else
            {
                _saveGameData.AiEmbelleshedDescriptions = true;
                var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);
                return new ProcessCommandResponse(
                "AI embellishments are on!  New location description: " + (currentLocation != null ? await currentLocation.GetDescription(_saveGameData) : "nothing"), _saveGameData, null, null, null);
            }
        }
    }
}