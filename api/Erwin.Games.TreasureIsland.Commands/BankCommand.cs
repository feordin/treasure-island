using Erwin.Games.TreasureIsland.Models;

using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    // Command Interface
    // It declares a method for executing a command
    public class BankCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        public BankCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _command = command;
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

            // check the current location
            var currentLocation = WorldData.Instance.GetLocation(_saveGameData.CurrentLocation);
            if (currentLocation == null || currentLocation.Name != "Bank")
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "These commands are only available at the bank.",
                    _saveGameData,
                    null,
                    null,
                    null,
                    null));
            }
            
            if (_command == "borrow")
            {
                if (_saveGameData?.Inventory?.Contains("collateral") == true)
                {
                    _saveGameData.Inventory.Remove("collateral");
                    _saveGameData.Inventory.Add("10 gold coins");
                    _saveGameData.Inventory.Add("Promissory note");
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                        "The bank manager smiles as he accepts your collateral and gives you a loan.",
                        _saveGameData,
                        null,
                        null,
                        null,
                        null));
                }
                else
                {
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                        "The bank manager says, 'I'm sorry, but I can't give you a loan without collateral.'",
                        _saveGameData,
                        null,
                        null,
                        null,
                        null));
                }
            }
            else if (_command == "steal" || _command == "rob" || _command == "take")
            {
                _saveGameData.CurrentLocation = "Jail";
                _saveGameData.CurrentDateTime.Add(new TimeSpan(0, 30, 0));
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "The small pile of coins looks too tempting, and you grab them when the bank manager isn't looking.\nUnfortunately, the armed watch you didn't notice catches you and you end up in jail.",
                    _saveGameData,
                    null,
                    null,
                    null,
                    null));
            }
            else
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "I'm sorry, this bank doesn't allow that.",
                    _saveGameData,
                    null,
                    null,
                    null,
                    null));
            }
        }
    }
}