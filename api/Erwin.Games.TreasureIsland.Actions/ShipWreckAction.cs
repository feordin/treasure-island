using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class ShipWreckAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public ShipWreckAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response?.saveGameData?.GetEvent("boat") == null)
            {
                _response?.saveGameData?.AddEvent("boat", "ShipWreck", _response.saveGameData.CurrentDateTime);
            }
            else
            {
                // Relocate the user to the ticket booth
                _response.saveGameData.CurrentLocation = "ShipWreckBeach";
                var currentLocation = WorldData.Instance?.GetLocation(_response.saveGameData.CurrentLocation);

                
                _response.saveGameData.CurrentDateTime += new TimeSpan(0, 2880, 0);

                _response.Message += "\n\nYou are awoken by the scream, 'Thar she blows!'.  You scramble topside and see an enourmous white whale off the starboard bow.  You have a terrible feeling that the captain is on his own personal quest, pursuing the beast.  The men harpoon it, but it alsmost seems the whale wanted that outcome as it pulls the ship directly into a terrible storm.  After what seems hours of waves, thunder and lightning, you finally black out.\n\n" + currentLocation?.Description;
                _response.ImageFilename = currentLocation?.Image;
            }
        }
    }
}