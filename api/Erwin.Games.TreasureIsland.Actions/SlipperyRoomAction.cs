using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class SlipperyRoomAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public SlipperyRoomAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response?.saveGameData?.Inventory == null || _response.saveGameData.Inventory.Count == 0)
            {
                _response.Message += "\n\nThe slippery walls glisten with moisture. You feel fortunate to have nothing to lose.";
                return;
            }

            // Check if already lost items here
            if (_response.saveGameData.GetEvent("slippery_room_loss") != null)
            {
                _response.Message += "\n\nThe slippery walls mock you - you've already lost everything here once.";
                return;
            }

            // Steal all items from inventory
            var lostItems = string.Join(", ", _response.saveGameData.Inventory);
            _response.saveGameData.Inventory.Clear();
            _response.saveGameData.AddEvent("slippery_room_loss", $"Lost items: {lostItems}", _response.saveGameData.CurrentDateTime);

            _response.Message += $"\n\nThe room's slippery walls cause you to lose your grip on everything! All your items slip away and vanish into cracks in the floor: {lostItems}. They are gone forever.";
        }
    }
}
