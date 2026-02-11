using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    /// <summary>
    /// Command to signal for rescue by lighting a fire at Rescue Beach.
    /// Requires matches and fuel (driftwood, lumber, or coal).
    /// </summary>
    public class SignalCommand : ICommand
    {
        private SaveGameData? _saveGameData;
        private IGameDataRepository _gameDataRepository;
        private string _command;
        private string? _param;

        private static readonly string[] FuelItems = { "driftwood", "lumber", "coal" };
        private const int MinDaysForPatrol = 30;
        private const int MaxScore = 3000;

        public SignalCommand(SaveGameData? saveGameData, IGameDataRepository gameDataRepository, string command, string? param = null)
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

            var currentLocation = WorldData.Instance.GetLocation(_saveGameData.CurrentLocation);

            // Must be at Rescue Beach
            if (currentLocation?.Name != "RescueBeach")
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You scan the horizon but this doesn't seem like a good spot to signal for rescue. You need to find an open beach facing the sea.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }

            // Check if already rescued
            if (_saveGameData.GetEvent("rescued") != null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You've already been rescued! Enjoy your treasures.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }

            // Check for matches
            bool hasMatches = _saveGameData.Inventory?.Contains("matches", StringComparer.OrdinalIgnoreCase) == true;

            // Check for fuel
            string? fuelItem = null;
            foreach (var fuel in FuelItems)
            {
                if (_saveGameData.Inventory?.Contains(fuel, StringComparer.OrdinalIgnoreCase) == true)
                {
                    fuelItem = fuel;
                    break;
                }
            }

            // If player has matches and fuel, light the fire
            if (hasMatches && fuelItem != null)
            {
                return LightSignalFire(fuelItem);
            }

            // Check if enough time has passed for patrol ships (alternative rescue)
            var startDate = new DateTime(1781, 6, 15); // Game start date
            var daysPassed = (_saveGameData.CurrentDateTime - startDate).Days;

            if (daysPassed >= MinDaysForPatrol)
            {
                return WaitForPatrol(daysPassed);
            }

            // Not enough to signal
            if (!hasMatches && fuelItem == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You wave your arms at the empty horizon, but there are no ships in sight. To signal for rescue, you'll need to build a fire. You need matches and fuel - driftwood from this beach, lumber from the shipwreck, or coal from deep in the caves.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
            else if (!hasMatches)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    $"You have {fuelItem} for a fire, but no way to light it. You need matches.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
            else
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    "You have matches but nothing to burn. You need fuel - driftwood from this beach, lumber from the shipwreck, or coal from the caves.",
                    _saveGameData,
                    null,
                    null,
                    null));
            }
        }

        private void ScoreInventoryTreasures()
        {
            if (_saveGameData?.Inventory == null || WorldData.Instance == null) return;

            foreach (var item in _saveGameData.Inventory)
            {
                var itemDetails = WorldData.Instance.GetItem(item);
                if (itemDetails?.Value > 0)
                {
                    var treasureEventName = $"scored_{itemDetails.Name}";
                    if (_saveGameData.GetEvent(treasureEventName) == null)
                    {
                        _saveGameData.Score += itemDetails.Value.Value;
                        _saveGameData.AddEvent(treasureEventName, $"Scored {itemDetails.Value.Value} points for {itemDetails.Name}", _saveGameData.CurrentDateTime);
                    }
                }
            }
        }

        private Task<ProcessCommandResponse?> LightSignalFire(string fuelItem)
        {
            // Consume the fuel
            _saveGameData!.Inventory!.RemoveAt(
                _saveGameData.Inventory.FindIndex(i => i.Equals(fuelItem, StringComparison.OrdinalIgnoreCase)));

            // Mark as rescued
            _saveGameData.AddEvent("rescued", "Rescued by signal fire", _saveGameData.CurrentDateTime);

            // Score any treasures still in inventory
            ScoreInventoryTreasures();

            // Build the victory message
            var score = _saveGameData.Score;
            var rating = GetScoreRating(score);

            var message = $"You strike a match and light the {fuelItem}. A column of smoke rises high into the clear Caribbean sky. " +
                "Within the hour, you spot a ship on the horizon changing course toward the island!\n\n" +
                "The crew of the merchant vessel 'Providence' rescues you from the beach. As you sail away from Treasure Island, " +
                "you reflect on your adventure.\n\n" +
                "=== CONGRATULATIONS! YOU HAVE BEEN RESCUED! ===\n\n" +
                $"Final Score: {score} out of {MaxScore} points\n" +
                $"Rating: {rating}\n\n";

            if (score == MaxScore)
            {
                message += "PERFECT SCORE! You recovered every treasure on the island! Uncle Herman would be proud.";
            }
            else if (score >= 2500)
            {
                message += "Excellent work! You recovered most of the island's treasures and return home wealthy.";
            }
            else if (score >= 1500)
            {
                message += "Good job! You found a respectable fortune and live comfortably.";
            }
            else if (score >= 500)
            {
                message += "You survived and found some treasure. There's always next time for the rest.";
            }
            else
            {
                message += "You escaped with your life, though the island's treasures remain hidden. Perhaps Uncle Herman's map holds more secrets...";
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message,
                _saveGameData,
                "rescued.png",
                null,
                null));
        }

        private Task<ProcessCommandResponse?> WaitForPatrol(int daysPassed)
        {
            // Mark as rescued
            _saveGameData!.AddEvent("rescued", "Rescued by patrol ship", _saveGameData.CurrentDateTime);

            // Score any treasures still in inventory
            ScoreInventoryTreasures();

            var score = _saveGameData.Score;
            var rating = GetScoreRating(score);

            var message = $"After {daysPassed} long days on the island, you've nearly given up hope. " +
                "But as you sit on Rescue Beach, staring at the horizon, you spot a sail! " +
                "A Royal Navy patrol ship has been searching for survivors of your shipwreck.\n\n" +
                "You wave frantically and they spot you! A longboat rows to shore and you are finally rescued.\n\n" +
                "=== CONGRATULATIONS! YOU HAVE BEEN RESCUED! ===\n\n" +
                $"Final Score: {score} out of {MaxScore} points\n" +
                $"Rating: {rating}\n\n";

            if (score >= 1500)
            {
                message += "Despite the long wait, you accumulated a fine fortune in treasures.";
            }
            else
            {
                message += "The long wait took its toll, but at least you survived to tell the tale.";
            }

            return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                message,
                _saveGameData,
                "rescued.png",
                null,
                null));
        }

        private string GetScoreRating(int score)
        {
            if (score >= 3000) return "Master Treasure Hunter";
            if (score >= 2500) return "Expert Explorer";
            if (score >= 2000) return "Seasoned Adventurer";
            if (score >= 1500) return "Capable Survivor";
            if (score >= 1000) return "Lucky Escapee";
            if (score >= 500) return "Novice Explorer";
            return "Castaway";
        }
    }
}
