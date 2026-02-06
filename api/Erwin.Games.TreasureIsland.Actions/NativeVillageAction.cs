using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class NativeVillageAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public NativeVillageAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            // Check if player has the dead black cat
            bool hasDeadCat = _response?.saveGameData?.Inventory?.Any(item =>
                item.Equals("deadBlackCat", StringComparison.OrdinalIgnoreCase) ||
                item.Equals("deadCat", StringComparison.OrdinalIgnoreCase) ||
                item.Equals("blackCat", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (hasDeadCat)
            {
                _response.Message += "\n\nThe natives see the dead black cat you carry and flee in terror, believing you possess dark magic. The village is now deserted.";
            }
            else
            {
                // Try last-chance escape before death
                if (LastChanceEscape.TryEscape(_response, "headhunter attack"))
                {
                    return;
                }

                // Player dies without the dead cat
                _response.saveGameData.AddEvent("GameOver", "Killed by headhunters", _response.saveGameData.CurrentDateTime);
                _response.Message += "\n\nThe headhunter natives spot you entering their village! Without any talisman to frighten them, they quickly surround you with spears. Your adventure ends here...";
            }
        }
    }
}
