using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    internal class ExamineCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        private string? _param;
        public ExamineCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
        {
            _saveGameData = saveGameData;
            _gameDataRepository = gameDataRepository;
            _command = command;
            _param = param;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            var item = WorldData.Instance?.GetItem(_param);
            if (string.IsNullOrEmpty(_param))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("What would you like to examine?", _saveGameData, null, null, null));
            }
            else if (_param == "bushes" && _saveGameData != null) // bushes are a special case which adds wallet to the list of items to alley end.
            {
                // check to see if the wallet was previously found
                var walletEvent = _saveGameData.GetEvent("wallet");
                if (walletEvent == null)
                {
                    var currentLocation = WorldData.Instance?.GetLocation(_saveGameData.CurrentLocation);
                    currentLocation?.AddItemToLocation(_saveGameData, "wallet");
                    _saveGameData.AddEvent("wallet", "You take a look at the bushes and find a wallet.", _saveGameData.CurrentDateTime);
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("You take a look at the bushes and find a wallet.", _saveGameData, null, null, null));
                }
            }
            else if (_param == "bookshelf" && _saveGameData != null)
            {
                                // check to see if the wallet was previously found
                var walletEvent = _saveGameData.GetEvent("book");
                if (walletEvent == null)
                {
                    var currentLocation = WorldData.Instance?.GetLocation(_saveGameData.CurrentLocation);
                    currentLocation?.AddItemToLocation(_saveGameData, "TheRepublic");
                    _saveGameData.AddEvent("TheRepublic", "Amazingly, wrapped in oil cloth, an old and valuable book of greek philosophy has survived.  This might be something the bank would consider collateral for a loan.", _saveGameData.CurrentDateTime);
                    return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("Amazingly, wrapped in oil cloth, an old and valuable book of greek philosophy has survived.  This might be something the bank would consider collateral for a loan.", _saveGameData, null, null, null));
                }
            }
            else if(_saveGameData?.Inventory?.Contains(_param, StringComparer.OrdinalIgnoreCase) == false && item?.IsTakeable == true)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse("You don't have a " + _param + " to examine.", _saveGameData, null, null, null));
            }

            return Task.FromResult<ProcessCommandResponse?>(
                new ProcessCommandResponse("You take a look at the " + _param + ".\n\n" + WorldData.Instance?.GetItem(_param)?.Description,_saveGameData, null, null, null));
        }
    }
}