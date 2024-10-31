using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class CheckLagoonSharkAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public CheckLagoonSharkAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response != null && _response?.saveGameData?.Events != null && _response.saveGameData.GetEvent("LagoonSwimming") == null)
            {
                _response.saveGameData?.AddEvent("LagoonSwimming", "Started swimming in the lagoon.", _response.saveGameData.CurrentDateTime);
                return;
            }

            if (_response != null && _response?.saveGameData != null)
            {
                var swimmingStartTime = _response.saveGameData.GetEvent("LagoonSwimming")?.EventDate;

                if (_response.saveGameData.CurrentDateTime - swimmingStartTime > new TimeSpan(0, 5, 0))
                {
                    _response.saveGameData.CurrentLocation = "LagoonShark";
                    var currentLocation = WorldData.Instance?.GetLocation(_response.saveGameData.CurrentLocation);
                    _response.Message += currentLocation?.Description;
                    _response.ImageFilename = currentLocation?.Image;
                    return;
                }
            }
        }
    }
}