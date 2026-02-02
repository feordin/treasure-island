using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class ThirstAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public ThirstAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            // Check if player has water in canteen
            bool hasWater = _response.saveGameData?.GetEvent("canteen_filled") != null;

            if (hasWater)
            {
                // Player has water - just warn them
                _response.Message += "\n\nThe salt in the air makes you very thirsty. Good thing you have water in your canteen!";
            }
            else
            {
                // No water - player dies of thirst!
                _response.saveGameData?.AddEvent("GameOver", "Died of thirst in the salt room", _response.saveGameData.CurrentDateTime);
                _response.Message += "\n\nThe salt in the air makes you desperately thirsty. Without any water, you collapse from dehydration. Your adventure ends here...";
            }
        }
    }
}
