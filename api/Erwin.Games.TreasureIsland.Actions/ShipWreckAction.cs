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
            if (_response?.saveGameData?.Inventory != null && _response.saveGameData.Inventory.Contains("ticket", StringComparer.OrdinalIgnoreCase))
            {
                // Remove the ticket from the inventory
                _response.saveGameData.Inventory.Remove("ticket");

                // Add a message to the response
                _response.Message += "\n\nYour ticket has been checked and removed from your inventory.";
            }
            else if (_response?.saveGameData != null)
            {
                // Relocate the user to the ticket booth
                _response.saveGameData.CurrentLocation = "TicketBooth";

                // Add a message to the response
                _response.Message += "\n\nNo free passage!. You have been escorted to the ticket booth.";
            }
        }
    }
}