using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class BuyCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        private string? _param;
        public BuyCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _command = command;
            _param = param;
        }
        public async Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null || WorldData.Instance == null)
            {
                return new ProcessCommandResponse(
                    "Unable to get the current game state or world data.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }
            var currentLocation = WorldData.Instance.GetLocation(_saveGameData.CurrentLocation);

            if (_param == null)
            {
                return new ProcessCommandResponse(
                    "What do you want to take?",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            var currentItems = currentLocation?.GetCurrentItems(_saveGameData);
            var itemDetails = WorldData.Instance?.GetItem(_param);

            if (currentItems?.Contains(_param + " pawned", StringComparer.OrdinalIgnoreCase) != null)
            {
                itemDetails = new Item() { Cost = 2, Description = "You pawned this off, but you can buy it back.", MustBuy = true, Name = _param + " pawned" };
            }
            else if (currentItems?.Contains(_param, StringComparer.OrdinalIgnoreCase) == false)
            {
                return new ProcessCommandResponse(
                    "The " + _param + " is not here.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            // we need another check here to make sure the is actually possible to take
            if (currentLocation?.Name != null &&
                _saveGameData != null &&
                itemDetails != null && itemDetails.MustBuy == true)
            {
                if (_saveGameData.Money >= itemDetails.Cost)
                {
                    _saveGameData.Inventory?.Add(_param);
                    _saveGameData.Money -= itemDetails.Cost;
                    // check if the saved game already has any changes to this location
                    currentLocation.RemoveItemFromLocation(_saveGameData, itemDetails.Name);

                    return new ProcessCommandResponse(
                        "You buy the " + _param + ".",
                        _saveGameData,
                        null,
                        null,
                        null);
                }
                else if (_saveGameData.Inventory?.Contains("wallet") == true)
                {
                    var walletEvent = _saveGameData.GetEvent("wallet");
                    
                    // you get one warning trying to use the wallet
                    if (walletEvent == null)
                    {
                        _saveGameData.AddEvent("wallet", "You try to use the wallet you found in the bushes, but remember it has a name in it, and the watch has it's eye on you.", _saveGameData.CurrentDateTime);
                        return new ProcessCommandResponse(
                            "You try to use the wallet you found in the bushes.  The watchman looks at you suspiciously.",
                            _saveGameData,
                            null,
                            null,
                            null);
                    }

                    // you get caught trying to use the wallet
                    _saveGameData.AddEvent("walletJail", "Unfortunately, the armed watch you didn't notice realizes that you are using money from a wallet that is not yours.  They take you jail.", _saveGameData.CurrentDateTime);
                    _saveGameData.RemoveEvent("wallet");
                    _saveGameData.CurrentLocation = "Jail";
                    currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);
                    _saveGameData?.CurrentDateTime.Add(new TimeSpan(0, 30, 0));
                    return new ProcessCommandResponse(
                        "Unfortunately, the armed watch you didn't notice realizes that you are using money from a wallet that is not yours.  They take you jail.",
                        _saveGameData,
                        currentLocation?.Image,
                        currentLocation != null ? await currentLocation.GetDescription(_saveGameData) : null,
                        null,
                        null);
                }
                else
                {
                    return new ProcessCommandResponse(
                                "You don't have enough money to buy the " + _param + ".  You'll need to get some more.",
                                _saveGameData,
                                null,
                                null,
                                null);
                }
            }
            else
            {
                return new ProcessCommandResponse(
                    "The " + itemDetails?.Name + "isn't for sale.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }
        }
    }
}