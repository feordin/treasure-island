using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class CheckBalloonTicketAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public CheckBalloonTicketAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response?.saveGameData?.Inventory != null && _response.saveGameData.Inventory.Contains("balloonticket", StringComparer.OrdinalIgnoreCase))
            {
                // Remove the ticket from the inventory
                _response.saveGameData.Inventory.RemoveAll(item => string.Equals(item, "Balloonticket", StringComparison.OrdinalIgnoreCase));

                // Add a message to the response
                _response.Message += "\n\nYour ticket has been checked and removed from your inventory.\n\n";
                _response.saveGameData.CurrentLocation = "Flying";
                var currentLocation = WorldData.Instance?.GetLocation(_response.saveGameData.CurrentLocation);
                _response.Message += currentLocation?.Description;

                _response.saveGameData.CurrentDateTime += TimeSpan.FromMinutes(120);
            }
            else if (_response?.saveGameData != null)
            {
                // Relocate the user to the ticket booth
                _response.saveGameData.CurrentLocation = "Airport";

                // Add a message to the response
                _response.Message += "\n\nNo free passage!. You have been escorted to the airport.";
            }
        }
    }
}