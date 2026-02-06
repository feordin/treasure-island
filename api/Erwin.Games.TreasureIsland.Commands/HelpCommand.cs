using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
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
            var helpMessage = basicHelp;

            // Check if this location has a help hint
            if (currentLocation?.Help != null && currentLocation.Help != "no")
            {
                // Check if this is a paid hint
                if (currentLocation.HelpCost > 0 && _saveGameData != null)
                {
                    var hintEventId = $"help_hint_{currentLocation.Name}";

                    // Only charge if player hasn't received this hint before
                    if (_saveGameData.GetEvent(hintEventId) == null)
                    {
                        // Deduct points for the hint
                        _saveGameData.Score -= currentLocation.HelpCost;

                        // Mark that player received this hint
                        _saveGameData.AddEvent(hintEventId,
                            $"Received paid help hint at {currentLocation.GetDisplayName()}",
                            _saveGameData.CurrentDateTime);

                        helpMessage += $"\n\n*** PAID HINT (-{currentLocation.HelpCost} points) ***\n" +
                            currentLocation.Help;
                    }
                    else
                    {
                        // Player already received this hint, show it again for free
                        helpMessage += "\n\n" + currentLocation.Help;
                    }
                }
                else
                {
                    // Free hint
                    helpMessage += "\n\n" + currentLocation.Help;
                }

                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                     helpMessage, _saveGameData, null, null, null));
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                helpMessage + "\n\nNo additional hints are available for this location.  You'll have to help yourself for now.", _saveGameData, null, null, null));
        }
    }
}
