using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class RemoveSwimmingEventAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public RemoveSwimmingEventAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response != null && _response?.saveGameData?.Events != null && _response.saveGameData.GetEvent("LagoonSwimming") == null)
            {
                return;
            }
            else if (_response != null && _response?.saveGameData != null)
            {
                _response.saveGameData.RemoveEvent("LagoonSwimming");
            }

            return;
        }
    }
}