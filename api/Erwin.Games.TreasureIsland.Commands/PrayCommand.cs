using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class PrayCommand : ICommand
    {
        private readonly SaveGameData _saveGameData;

        public PrayCommand(SaveGameData saveGameData)
        {
            _saveGameData = saveGameData;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            ProcessCommandResponse? response;
            
            if (_saveGameData?.CurrentLocation?.Equals("church", StringComparison.OrdinalIgnoreCase) == true)
            {
                response = new ProcessCommandResponse(
                    message: "You receive a vision about Uncle Herman trapped on Treasure Island. You see a hidden wallet in some bushes which will help.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null
                );
            }
            else
            {
                response = new ProcessCommandResponse(
                    message: "While it is admirable to pray anywhere and at all times, you will have more luck in the church.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null
                );
            }
            

            return Task.FromResult<ProcessCommandResponse?>(response);
        }
    }
}