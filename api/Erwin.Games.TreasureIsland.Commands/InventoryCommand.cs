using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class InventoryCommand : ICommand
    {
        private SaveGameData? _saveGameData;

        public InventoryCommand(SaveGameData? saveGameData)
        {
            _saveGameData = saveGameData;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            if (_saveGameData?.Inventory == null || _saveGameData.Inventory.Count == 0)
            {
                return Task.FromResult<ProcessCommandResponse?>(
                    new ProcessCommandResponse(
                        "You are not carrying anything. You can also check your inventory at any time in the side panel.",
                        _saveGameData,
                        null,
                        null,
                        null));
            }

            var itemNames = _saveGameData.Inventory.Select(item =>
                WorldData.Instance?.GetItemDisplayName(item) ?? item);

            var message = "You are carrying:\n\n" +
                string.Join("\n", itemNames) +
                "\n\nRemember, you can also see your inventory at any time on the screen, or on mobile by opening the side panel.";

            return Task.FromResult<ProcessCommandResponse?>(
                new ProcessCommandResponse(
                    message,
                    _saveGameData,
                    null,
                    null,
                    null));
        }
    }
}
