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
                    return;
                }
                else 
                {
                    _response.saveGameData.CurrentLocation = "Airport";
                    var currentLocation = WorldData.Instance?.GetLocation(_response.saveGameData.CurrentLocation);
                    _response.Message += "The flight was amazing and the wind carries you back safely to where you launched.\n\n" + currentLocation?.Description;
                    return;
                }
            }
        }
    }
}