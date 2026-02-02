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
            // TODO: Future implementation - check for "ate_mushrooms" event
            // If player eats mushrooms, they should die from poisoning
            // For now, just show warning message

            _response.Message += "\n\nThere are colorful mushrooms growing here. They look tempting but could be poisonous.";
        }
    }
}
