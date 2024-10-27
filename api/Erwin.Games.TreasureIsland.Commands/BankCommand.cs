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

            // check the current location
            var currentLocation = WorldData.Instance.GetLocation(_saveGameData.CurrentLocation);
            if (currentLocation == null || currentLocation.Name != "Bank")
            {
                return new ProcessCommandResponse(
                    "These commands are only available at the bank.",
                    _saveGameData,
                    null,
                    null,
                    null,
                    null);
            }
            
            if (_command == "borrow")
            {
                if (_saveGameData.GetEvent("BankLoan") != null)
                {
                    return new ProcessCommandResponse(
                        "The bank manager says, 'I'm sorry, but you already have a loan from the bank.'",
                        _saveGameData,
                        null,
                        null,
                        null,
                        null);
                }
                if (_saveGameData?.Inventory?.Contains("TheRepublic", StringComparer.OrdinalIgnoreCase) == true)
                {
                    _saveGameData.Inventory.Remove("therepublic");
                    _saveGameData.Money += 10;
                    _saveGameData.Inventory.Add("Promissory note");
                    _saveGameData.AddEvent("BankLoan", "You borrowed 10 gold from the bank.", _saveGameData.CurrentDateTime);
                    return new ProcessCommandResponse(
                        "The bank manager smiles as he accepts your collateral and gives you a loan.",
                        _saveGameData,
                        null,
                        null,
                        null,
                        null);
                }
                else
                {
                    return new ProcessCommandResponse(
                        "The bank manager says, 'I'm sorry, but I can't give you a loan without collateral.'",
                        _saveGameData,
                        null,
                        null,
                        null,
                        null);
                }
            }
            else if (_command == "steal" || _command == "rob" || _command == "take")
            {
                _saveGameData.CurrentLocation = "JailCell";
                currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);
                _saveGameData?.CurrentDateTime.Add(new TimeSpan(0, 30, 0));
                return new ProcessCommandResponse(
                    "The small pile of coins looks too tempting, and you grab them when the bank manager isn't looking.\nUnfortunately, the armed watch you didn't notice catches you and you end up in jail.",
                    _saveGameData,
                    currentLocation?.Image,
                    currentLocation != null ? await currentLocation.GetDescription(_saveGameData) : null,
                    null,
                    null);
            }
            else
            {
                return new ProcessCommandResponse(
                    "I'm sorry, this bank doesn't allow that.",
                    _saveGameData,
                    null,
                    null,
                    null,
                    null);
            }
        }
    }
}