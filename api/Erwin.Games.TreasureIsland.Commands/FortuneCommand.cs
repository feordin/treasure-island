using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class FortuneCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        
        public FortuneCommand(SaveGameData? saveGameData)
        {
            _saveGameData = saveGameData;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            if (currentLocation?.Name == "FortuneTeller")
            {
                if (_saveGameData?.Money >= 2)
                {
                    _saveGameData.Money -= 2;
                    return new ProcessCommandResponse(
                        "Madame Isadora, La Vidente, caresses your palm and then speaks: " + await currentLocation.GetFortune(), _saveGameData, null, null, null);
                }
                else
                {
                    return new ProcessCommandResponse(
                        "Madame Isadora, La Vidente, looks at you expectantly.  'The spirits require 5 coins to speak, dear traveler.'", _saveGameData, null, null, null);
                }
            }

            return new ProcessCommandResponse(
                "There is no one here to read your fortune.  You'll need to go to a different location.", _saveGameData, null, null, null);
        }
    }
}