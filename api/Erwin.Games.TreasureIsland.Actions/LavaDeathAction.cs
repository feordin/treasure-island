using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class LavaDeathAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public LavaDeathAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            _response.saveGameData.AddEvent("GameOver", "Fell into lava", _response.saveGameData.CurrentDateTime);
            _response.Message += "\n\nThe intense heat from the bubbling lava is overwhelming. Before you can turn back, the ground beneath you crumbles and you fall into the molten rock. Game Over.";
        }
    }
}
