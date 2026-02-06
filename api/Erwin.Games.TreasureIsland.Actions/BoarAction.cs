using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class BoarAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public BoarAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            // Check if boar has already been fed
            if (_response.saveGameData.GetEvent("boar_fed") != null)
            {
                // Boar is gone, no action needed
                return;
            }

            // Check if player has donuts in inventory
            bool hasDonuts = _response.saveGameData.Inventory?.Any(item =>
                item.Equals("donuts", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (hasDonuts)
            {
                // Remove donuts from inventory
                _response.saveGameData.Inventory = _response.saveGameData.Inventory
                    .Where(item => !item.Equals("donuts", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Mark boar as fed
                _response.saveGameData.AddEvent("boar_fed", "Fed donuts to the boar", _response.saveGameData.CurrentDateTime);

                _response.Message += "\n\nA massive wild boar blocks your path! You quickly toss the donuts at it. The boar snorts happily and devours them, then wanders off into the brush, leaving the path clear.";
            }
            else
            {
                // Try last-chance escape before death
                if (LastChanceEscape.TryEscape(_response, "wild boar attack"))
                {
                    return;
                }

                // Player dies
                _response.saveGameData.AddEvent("GameOver", "Killed by wild boar", _response.saveGameData.CurrentDateTime);
                _response.Message += "\n\nA massive wild boar charges at you! Without anything to distract it, you have no chance. Game Over.";
            }
        }
    }
}
