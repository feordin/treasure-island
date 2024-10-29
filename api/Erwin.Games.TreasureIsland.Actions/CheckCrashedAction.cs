using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class CheckCrashedAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public CheckCrashedAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response != null && _response?.saveGameData?.Events != null && _response.saveGameData.GetEvent("Flying") == null)
            {
                // Remove the ticket from the inventory
                _response?.saveGameData?.Inventory?.RemoveAll(item => string.Equals(item, "Balloonticket", StringComparison.OrdinalIgnoreCase));

                // Add a message to the response
                _response.Message += "\n\nYour ticket has been checked and removed from your inventory.\n\n";

                _response.saveGameData?.AddEvent("Flying", "First we allow one command without doing anything.", _response.saveGameData.CurrentDateTime);
                return;
            }

            _response?.saveGameData?.RemoveEvent("Flhing");

            if (_response != null && _response?.saveGameData != null)
            {
                var random = new Random();
                var crashed = random.NextDouble() < 0.2;

                if (crashed)
                {
                    // Add a message to the response
                    _response.saveGameData.CurrentLocation = "Crashed";
                    var currentLocation = WorldData.Instance?.GetLocation(_response.saveGameData.CurrentLocation);
                    _response.Message += currentLocation?.Description;
                    _response.ImageFilename = currentLocation?.Image;
                    return;
                }
                else 
                {
                    _response.saveGameData.CurrentLocation = "Airport";
                    var currentLocation = WorldData.Instance?.GetLocation(_response.saveGameData.CurrentLocation);
                    _response.Message += "\n\nThe flight was amazing and the wind carries you back safely to where you launched.\n\n" + currentLocation?.Description;
                    _response.ImageFilename = currentLocation?.Image;
                    return;
                }
            }
        }
    }
}