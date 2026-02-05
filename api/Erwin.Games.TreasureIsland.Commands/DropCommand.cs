using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class DropCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        private string? _param;
        private const int MaxScore = 3000;

        public DropCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _command = command;
            _param = param;
        }
        public Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null || WorldData.Instance == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "Unable to get the current game state or world data.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }

            // check the current location, the drop command works differently in the pawn shop
            var currentLocation = WorldData.Instance.GetLocation(_saveGameData.CurrentLocation);

            if (_param == null){
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "What do you want to drop?",
                    _saveGameData,
                    null,
                    null,
                    null));
            }

            if (currentLocation?.Name == "PawnShop")
            {
                var pawnCommand = new PawnCommand(_saveGameData, _gameDataRepository, _command, _param);
                return pawnCommand.Execute();
            }

            var currentItems = currentLocation?.GetCurrentItems(_saveGameData);
            var itemDetails = WorldData.Instance?.GetItem(_param);

            if(_saveGameData?.Inventory?.Contains(_param, StringComparer.OrdinalIgnoreCase) == true &&
                currentLocation?.Name != null)
            {
                _saveGameData?.Inventory?.RemoveAt(_saveGameData.Inventory.FindIndex(n => n.Equals(_param, StringComparison.OrdinalIgnoreCase)));

                if (_param == "wallet" && _saveGameData != null && currentLocation?.Name == "ConstablesOffice")
                {
                    _saveGameData.Money += 10;
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "The constable looks surprised, but praises you for your honesty.  He begrudgingly hands over the reward of 10 gold.",
                    _saveGameData,
                    null,
                    null,
                    null));
                }

                // Check if dropping a treasure at Rescue Beach for points
                if (currentLocation?.Name == "RescueBeach" && itemDetails?.Value > 0)
                {
                    return HandleTreasureDrop(currentLocation, itemDetails);
                }

                currentLocation?.AddItemToLocation(_saveGameData, _param);

                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You drop the " + _param + ".",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
            else
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You don't have a " + _param + " in your inventory.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
        }

        private Task<ProcessCommandResponse?> HandleTreasureDrop(Location currentLocation, Item itemDetails)
        {
            var treasureEventName = $"scored_{itemDetails.Name}";

            // Check if this treasure has already been scored
            if (_saveGameData!.GetEvent(treasureEventName) != null)
            {
                currentLocation.AddItemToLocation(_saveGameData, _param!);
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    $"You drop the {itemDetails.Name}. You've already received points for this treasure.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }

            // Award points
            var pointsAwarded = itemDetails.Value!.Value;
            _saveGameData.Score += pointsAwarded;

            // Mark this treasure as scored
            _saveGameData.AddEvent(treasureEventName, $"Scored {pointsAwarded} points for {itemDetails.Name}", _saveGameData.CurrentDateTime);

            // Add item to location
            currentLocation.AddItemToLocation(_saveGameData, _param!);

            var message = $"You carefully place the {itemDetails.Name} on the beach. " +
                $"This treasure is worth {pointsAwarded} points!\n\n" +
                $"Your score is now {_saveGameData.Score} out of {MaxScore} points.";

            // Check for perfect score
            if (_saveGameData.Score >= MaxScore)
            {
                message += "\n\n*** INCREDIBLE! You have recovered ALL the treasures of the island! ***\n" +
                    "You are a Master Treasure Hunter! Now signal for rescue to complete your adventure.";
            }
            else if (_saveGameData.Score >= 2500)
            {
                message += "\n\nYou're so close to recovering all the treasures!";
            }
            else if (_saveGameData.Score >= 1500)
            {
                message += "\n\nYou've amassed quite a fortune! But there's more treasure to find...";
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message,
                _saveGameData,
                null,
                null,
                null));
        }
    }
}