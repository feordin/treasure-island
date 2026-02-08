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
                // Determine chance the boar is home based on how many times player has passed through
                var boarEvent = _response.saveGameData.GetEvent("boar");
                int encounterCount = 0;
                if (boarEvent != null)
                {
                    int.TryParse(boarEvent.Description, out encounterCount);
                }

                // Chance of boar appearing is (encounterCount + 1) in 10
                int chanceOutOfTen = Math.Min(encounterCount + 1, 10);
                var random = new Random();
                bool boarIsHome = random.Next(10) < chanceOutOfTen;

                if (boarIsHome)
                {
                    // Try last-chance escape before death
                    if (LastChanceEscape.TryEscape(_response, "wild boar attack"))
                    {
                        return;
                    }

                    // Player dies
                    _response.saveGameData.AddEvent("GameOver", "Killed by wild boar", _response.saveGameData.CurrentDateTime);
                    _response.saveGameData.CurrentLocation = "GameOver";
                    _response.Message += "\n\nA massive wild boar charges at you! Without anything to distract it, you have no chance. Game Over.";
                }
                else
                {
                    // Boar wasn't home, but increment the encounter count
                    if (boarEvent != null)
                    {
                        boarEvent.Description = (encounterCount + 1).ToString();
                    }
                    else
                    {
                        _response.saveGameData.AddEvent("boar", "1", _response.saveGameData.CurrentDateTime);
                    }

                    _response.Message += "\n\nYou hear snorting in the distance but the wild boar doesn't seem to be home. You sneak through quickly.";
                }
            }
        }
    }
}
