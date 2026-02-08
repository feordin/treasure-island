using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class MushroomDeathAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public MushroomDeathAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            var saveData = _response.saveGameData;

            // If the player ate the mushrooms, they die from poisoning
            if (saveData?.GetEvent("ate_mushrooms") != null)
            {
                // Set previous location for last-chance escape
                saveData.PreviousLocation = saveData.CurrentLocation;

                // Try last-chance escape before death
                if (LastChanceEscape.TryEscape(_response, "mushroom poisoning"))
                {
                    return;
                }

                saveData.AddEvent("GameOver", "Poisoned by eating mushrooms in the mushroom room", saveData.CurrentDateTime);
                saveData.CurrentLocation = "GameOver";

                _response.Message += "\n\nThe mushroom poison courses through your veins. The room spins as you collapse. Your adventure ends here.";
                return;
            }

            _response.Message += "\n\nThere are colorful mushrooms growing here. They look tempting but could be poisonous.";
        }
    }
}
