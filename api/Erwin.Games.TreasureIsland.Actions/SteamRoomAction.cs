using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class SteamRoomAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public SteamRoomAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            // Check if player has matches in inventory
            bool hasMatches = _response.saveGameData.Inventory?.Any(item =>
                item.Equals("matches", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (hasMatches)
            {
                // Only add event once
                if (_response.saveGameData.GetEvent("wet_matches") == null)
                {
                    _response.saveGameData.AddEvent("wet_matches", "Matches got wet in steam room", _response.saveGameData.CurrentDateTime);
                    _response.Message += "\n\nThe thick steam permeates everything. Your matches have gotten wet and may not light properly now.";
                }
                else
                {
                    _response.Message += "\n\nSteam fills the room, creating a thick humid fog.";
                }
            }
            else
            {
                _response.Message += "\n\nSteam fills the room, creating a thick humid fog.";
            }
        }
    }
}
