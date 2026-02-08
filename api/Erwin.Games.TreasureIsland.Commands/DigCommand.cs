using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class DigCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;

        public DigCommand(SaveGameData? saveGameData)
        {
            _saveGameData = saveGameData;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            if (_saveGameData?.Inventory?.Contains("shovel", StringComparer.OrdinalIgnoreCase) == false)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You try digging with your bare hands, but can't make much progress.  If you only had a shovel.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            if (_saveGameData?.CurrentLocation?.Equals("TwinPalms", StringComparison.OrdinalIgnoreCase) == true)
            {
                // check to see if the item has already been found
                var itemEvent = _saveGameData.GetEvent("TreasureChestReveal");
                if (itemEvent == null)
                {
                    currentLocation?.AddItemToLocation(_saveGameData, "TreasureChest");
                    _saveGameData?.AddEvent("TreasureChestReveal", "Digging is successfull!  You find a treasure chest!", _saveGameData.CurrentDateTime);
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("Digging is successfull!  You find a treasure chest!", _saveGameData, null, null, null));
                }
                else
                {
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                        message: "You dig more around where you found the treasure chest, but don't find anything else.",
                        saveGameData: _saveGameData,
                        imageFilename: null,
                        locationDescription: null,
                        commandHistory: null
                    ));
                }
            }
            else if (_saveGameData?.CurrentLocation?.Equals("RescueBeach", StringComparison.OrdinalIgnoreCase) == true)
            {
                var itemEvent = _saveGameData.GetEvent("PotOfGoldReveal");
                if (itemEvent == null)
                {
                    currentLocation?.AddItemToLocation(_saveGameData, "potOfGold");
                    _saveGameData?.AddEvent("PotOfGoldReveal", "Digging is successful! You find a pot of gold!", _saveGameData.CurrentDateTime);
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("You dig into the sand and your shovel strikes something hard. It's a pot of gold!", _saveGameData, null, null, null));
                }
                else
                {
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                        message: "You dig more around where you found the pot of gold, but don't find anything else.",
                        saveGameData: _saveGameData,
                        imageFilename: null,
                        locationDescription: null,
                        commandHistory: null
                    ));
                }
            }
            else
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Digging in the dirt is fun, but you don't find anything.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null
                ));
            }
        }
    }
}