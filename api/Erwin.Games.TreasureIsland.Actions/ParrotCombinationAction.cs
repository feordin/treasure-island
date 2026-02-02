using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class ParrotCombinationAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public ParrotCombinationAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            // Only reveal combination once
            if (_response.saveGameData.GetEvent("learned_combination") == null)
            {
                _response.saveGameData.AddEvent("learned_combination", "7-23-42", _response.saveGameData.CurrentDateTime);
                _response.Message += "\n\nA parrot chained to the wall squawks numbers repeatedly: '7-23-42'. You memorize them - they sound like a safe combination!";
            }
            else
            {
                _response.Message += "\n\nThe parrot continues to squawk the same numbers: '7-23-42'.";
            }
        }
    }
}
