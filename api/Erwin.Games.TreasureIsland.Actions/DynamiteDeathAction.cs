using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class DynamiteDeathAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public DynamiteDeathAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            // Check if player has dynamite in inventory (just picked it up)
            bool hasDynamite = _response.saveGameData.Inventory?.Any(item =>
                item.Equals("dynamite", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (hasDynamite)
            {
                // Try last-chance escape before death
                if (LastChanceEscape.TryEscape(_response, "dynamite explosion"))
                {
                    // Remove the dynamite since it exploded
                    _response.saveGameData.Inventory = _response.saveGameData.Inventory?
                        .Where(item => !item.Equals("dynamite", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    return;
                }

                _response.saveGameData.AddEvent("GameOver", "Killed by unstable dynamite", _response.saveGameData.CurrentDateTime);
                _response.Message += "\n\nAs you pick up the old dynamite, it becomes unstable and explodes in your hands! Game Over.";
            }
            else
            {
                _response.Message += "\n\nThere is some old dynamite here. It looks extremely unstable - touching it would be very dangerous.";
            }
        }
    }
}
