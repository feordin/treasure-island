using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class CheckLostInTheJungleAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public CheckLostInTheJungleAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response?.saveGameData?.GetEvent("LostInTheJungle") == null)
            {
                _response?.saveGameData?.AddEvent("LostInTheJungle", "You are lost in the jungle.", _response.saveGameData.CurrentDateTime);
                return;
            }
            else
            {
                _response.saveGameData.CurrentDateTime += new TimeSpan(0, 1, 0);

                if (_response?.saveGameData.CurrentDateTime >= _response?.saveGameData?.GetEvent("LostInTheJungle")?.EventDate + new TimeSpan(0, 5, 0))
                {
                    Random random = new Random();
                    int randomNumber = random.Next(1, 5); // Generates a number between 1 (inclusive) and 5 (exclusive)
                    Location? currentLocation = null;
                    switch (randomNumber)
                    {
                        case 1:
                            currentLocation = WorldData.Instance?.GetLocation("JungleFork");
                            break;
                        case 2:
                            currentLocation = WorldData.Instance?.GetLocation("ZigZagTrail");
                            break;
                        case 3:
                            currentLocation = WorldData.Instance?.GetLocation("ShipWreckBeach");
                            break;
                        case 4:
                            currentLocation = WorldData.Instance?.GetLocation("JungleTrail");
                            break;
                    }
                    _response.saveGameData.CurrentLocation = currentLocation?.Name;
                    _response.Message = currentLocation?.Description;
                    _response.ImageFilename = currentLocation?.Image;
                    _response?.saveGameData?.RemoveEvent("LostInTheJungle");
                }
            }
        }
    }
}