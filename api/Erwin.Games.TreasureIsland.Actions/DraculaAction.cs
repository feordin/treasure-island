using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class DraculaAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public DraculaAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            // Check if Dracula has already been killed
            if (_response?.saveGameData?.GetEvent("killed_dracula") != null)
            {
                _response.Message += "\n\nThe coffin is empty.";
                return;
            }

            // Check the time of day
            var currentHour = _response.saveGameData.CurrentDateTime.Hour;
            bool isNight = currentHour >= 20 || currentHour < 6;

            if (isNight)
            {
                // Try last-chance escape before death
                if (LastChanceEscape.TryEscape(_response, "Dracula attack"))
                {
                    return;
                }

                // Dracula is awake - player dies
                _response.saveGameData.AddEvent("GameOver", "Killed by Dracula", _response.saveGameData.CurrentDateTime);
                _response.Message += "\n\nAs you enter the room, you see a dark coffin. Suddenly, the lid bursts open and Count Dracula rises before you, his eyes glowing red in the darkness! Before you can react, he lunges at your throat with supernatural speed. Your adventure ends here in the darkness...";
            }
            else
            {
                // Dracula is sleeping during the day - player is safe
                _response.Message += "\n\nYou cautiously approach the ornate coffin. Inside, you see the infamous Count Dracula lying motionless, pale as death. He appears to be in a deep slumber, vulnerable during the daylight hours. A wooden stake lies nearby...";
            }
        }
    }
}
