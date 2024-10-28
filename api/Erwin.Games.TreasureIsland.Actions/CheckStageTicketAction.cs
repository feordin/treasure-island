using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class CheckStageTicketAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public CheckStageTicketAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response?.saveGameData?.Events != null && _response.saveGameData.GetEvent("GameOver") != null)
            {
                _response.Message += "\n\nThere is nothing more to do. If you want to keep playing, load a save game or start over.";
                return;
            }

            if (_response?.saveGameData?.Inventory != null && _response.saveGameData.Inventory.Contains("stageticket", StringComparer.OrdinalIgnoreCase))
            {
                // Remove the ticket from the inventory
                _response.saveGameData.Inventory.RemoveAll(item => string.Equals(item, "stageticket", StringComparison.OrdinalIgnoreCase));

                // Add a message to the response
                _response.Message += "\n\nYour ticket has been checked and removed from your inventory.\n\nYou enjoy the plush seats and view of the country side as the coach pulls away from the depot.  Uncle Herman is a resourceful man, you're sure he'll be fine and be home in no time.  Game Over.";
                _response.saveGameData.AddEvent("GameOver", "You took the stage home to farm.", _response.saveGameData.CurrentDateTime);
            }
            else if (_response?.saveGameData != null)
            {
                // Relocate the user to the ticket booth
                _response.saveGameData.CurrentLocation = "StageCoachDepot";

                // Add a message to the response
                _response.Message += "\n\nNo free passage!. You have been escorted to the depot.";
            }
        }
    }
}