using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class FissureWaterAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public FissureWaterAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response.saveGameData.GetEvent("fissure_filled") != null)
            {
                _response.Message += "\n\nThe fissure is now filled with cold water from the melted ice. You could swim across to the ledge on the other side.";
            }
        }
    }
}
