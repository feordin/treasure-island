using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class WishCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly string? _wishType;
        private static readonly Random _random = new();

        private static readonly string[] TreasureHints = new[]
        {
            "You see a vision of treasure glittering in an office, hidden in a mansion deep in the jungle.",
            "You see a vision of riches buried beneath rescue beach. You'll need a shovel to dig it up.",
            "You see a vision of treasure in a dark place called Goblin Valley. Bring a light source.",
            "You see a vision of treasure at Deadman's Gulch. It is dark there, so bring something to light your way.",
            "You see a vision of treasure hidden in a fissure room, deep in the caves beneath the island."
        };

        public WishCommand(SaveGameData? saveGameData, string? wishType)
        {
            _saveGameData = saveGameData;
            _wishType = wishType?.ToLowerInvariant();
        }

        public async Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null)
            {
                return new ProcessCommandResponse(
                    "Unable to get the current game state.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            // Check if player has the monkey's paw
            if (_saveGameData.Inventory?.Contains("monkeysPaw", StringComparer.OrdinalIgnoreCase) != true)
            {
                return new ProcessCommandResponse(
                    "You don't have anything to wish upon. Perhaps the fortune teller has something that could help.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            // Check if player has already used their wish
            if (_saveGameData.GetEvent("MonkeysPawWish") != null)
            {
                return new ProcessCommandResponse(
                    "The monkey's paw lies still and lifeless. Its single wish has been granted, and it has no more power to give.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            // Validate wish type
            if (string.IsNullOrEmpty(_wishType))
            {
                return new ProcessCommandResponse(
                    "The monkey's paw trembles in anticipation. What do you wish for? You may wish for 'money', 'treasure', 'food', or 'help'.",
                    _saveGameData,
                    null,
                    null,
                    null);
            }

            string message;
            var currentLocation = WorldData.Instance?.GetLocation(_saveGameData.CurrentLocation);

            switch (_wishType)
            {
                case "money":
                case "gold":
                case "coins":
                    _saveGameData.Money += 10;
                    _saveGameData.CurrentLocation = "JailCell";
                    _saveGameData.AddEvent("MonkeysPawWish", "Wished for money - got gold but ended up in jail.", _saveGameData.CurrentDateTime);
                    var jailLocation = WorldData.Instance?.GetLocation("JailCell");
                    message = "The monkey's paw curls its finger, and suddenly you find 10 gold coins in your pocket!\n\n" +
                              "But wait... the town watch appears! 'Thief! That's stolen gold!' they cry.\n\n" +
                              "Before you can protest, you're dragged off to jail. The wish has been granted... at a price.";
                    return new ProcessCommandResponse(
                        message,
                        _saveGameData,
                        jailLocation?.Image,
                        jailLocation != null ? await jailLocation.GetDescription(_saveGameData) : null,
                        null);

                case "treasure":
                case "riches":
                    _saveGameData.AddEvent("MonkeysPawWish", "Wished for treasure - received a vision of treasure location.", _saveGameData.CurrentDateTime);
                    var hintIndex = _random.Next(TreasureHints.Length);
                    message = "The monkey's paw curls its finger, and your vision blurs...\n\n" +
                              TreasureHints[hintIndex] + "\n\n" +
                              "The vision fades. The paw lies still, its power spent.";
                    break;

                case "food":
                case "donuts":
                case "hunger":
                    _saveGameData.Inventory ??= new List<string>();
                    if (!_saveGameData.Inventory.Contains("donuts", StringComparer.OrdinalIgnoreCase))
                    {
                        _saveGameData.Inventory.Add("donuts");
                    }
                    _saveGameData.AddEvent("MonkeysPawWish", "Wished for food - received donuts.", _saveGameData.CurrentDateTime);
                    message = "The monkey's paw curls its finger, and a box of fresh donuts appears in your hands!\n\n" +
                              "They smell delicious, and strangely, nothing bad seems to happen... yet.\n\n" +
                              "The paw lies still, its power spent.";
                    break;

                case "help":
                case "guidance":
                case "advice":
                    _saveGameData.AddEvent("MonkeysPawWish", "Wished for help - received survival hints.", _saveGameData.CurrentDateTime);
                    message = "The monkey's paw curls its finger, and whispers fill your mind...\n\n" +
                              "'Listen well, traveler. On your journey you will face dangers:\n" +
                              "- Bring DONUTS to appease the hungry...\n" +
                              "- Carry MATCHES to light your way in darkness...\n" +
                              "- A wooden STAKE will be needed to defeat an ancient evil that lurks in shadows...'\n\n" +
                              "The whispers fade. The paw lies still, its power spent.";
                    break;

                case "rescue":
                case "escape":
                case "safety":
                case "home":
                case "beach":
                    _saveGameData.AddEvent("MonkeysPawWish", "Wished for rescue - teleported to Rescue Beach.", _saveGameData.CurrentDateTime);
                    _saveGameData.CurrentLocation = "RescueBeach";
                    var rescueLocation = WorldData.Instance?.GetLocation("RescueBeach");
                    message = "The monkey's paw curls its finger, and the world around you dissolves into mist...\n\n" +
                              "When the mist clears, you find yourself standing on a familiar sandy beach. " +
                              "The sound of waves and the cry of seagulls greet you. You've been transported to Rescue Beach!\n\n" +
                              "The paw lies still in your hand, its power spent. You got your wish, but at what cost? " +
                              "Any treasures you left behind in the caves are now beyond your reach.";
                    return new ProcessCommandResponse(
                        message,
                        _saveGameData,
                        rescueLocation?.Image,
                        rescueLocation != null ? rescueLocation.Description : null,
                        null);

                default:
                    return new ProcessCommandResponse(
                        "The monkey's paw does not understand your wish. You may wish for 'money', 'treasure', 'food', or 'help'.",
                        _saveGameData,
                        null,
                        null,
                        null);
            }

            return new ProcessCommandResponse(
                message,
                _saveGameData,
                currentLocation?.Image,
                currentLocation?.Description,
                null);
        }
    }
}
