using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class BattlefieldDeathAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public BattlefieldDeathAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            _response.saveGameData.AddEvent("GameOver", "Killed by mine on battlefield", _response.saveGameData.CurrentDateTime);
            _response.Message += "\n\nYou step onto the battlefield. Before you can react, you hear a click beneath your foot - an unexploded mine! A massive explosion engulfs you. Game Over.";
        }
    }
}
