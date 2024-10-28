using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class PrayCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;

        public PrayCommand(SaveGameData? saveGameData)
        {
            _saveGameData = saveGameData;
        }

        public async Task<ProcessCommandResponse?> Execute()
        {
            ProcessCommandResponse? response;

            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData?.CurrentLocation);

            if (_saveGameData?.CurrentLocation?.Equals("church", StringComparison.OrdinalIgnoreCase) == true)
            {
                string prayer = "You receive a vision about Uncle Herman trapped on Treasure Island. You see a hidden wallet in some bushes which will help.";

                if (currentLocation != null)
                {
                    string? aiPrayer = await currentLocation.Pray();
                    if (!string.IsNullOrEmpty(aiPrayer))
                    {
                        prayer = aiPrayer;
                    }
                }
                response = new ProcessCommandResponse(
                    message: prayer,
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


            return response;
        }
    }
}