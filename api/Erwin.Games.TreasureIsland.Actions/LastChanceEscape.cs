using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    /// <summary>
    /// Helper class that provides "last chance" escape mechanics for death trap locations.
    /// If a player has an unused monkey's paw or Aladdin's lamp, they can escape certain death.
    /// </summary>
    public static class LastChanceEscape
    {
        /// <summary>
        /// Attempts to escape from a deadly situation using the monkey's paw or lamp.
        /// Returns true if escape was successful, false if the player should die.
        /// </summary>
        public static bool TryEscape(ProcessCommandResponse response, string deathDescription)
        {
            var saveData = response?.saveGameData;
            if (saveData == null) return false;

            // Check if player has previous location to escape to
            if (string.IsNullOrEmpty(saveData.PreviousLocation))
            {
                return false;
            }

            // Try monkey's paw first (it's the backup item)
            if (TryUseMonkeysPaw(response))
            {
                return true;
            }

            // Try Aladdin's lamp
            if (TryUseLamp(response))
            {
                return true;
            }

            return false;
        }

        private static bool TryUseMonkeysPaw(ProcessCommandResponse response)
        {
            var saveData = response.saveGameData;

            // Check if player has the monkey's paw
            bool hasPaw = saveData.Inventory?.Any(item =>
                item.Equals("monkeysPaw", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasPaw) return false;

            // Check if the paw's wish has already been used
            if (saveData.GetEvent("MonkeysPawWish") != null)
            {
                return false;
            }

            // Use the paw to escape!
            var previousLocation = saveData.PreviousLocation;
            var prevLocationData = WorldData.Instance?.GetLocation(previousLocation);

            saveData.AddEvent("MonkeysPawWish",
                "Used monkey's paw to escape certain death",
                saveData.CurrentDateTime);

            saveData.CurrentLocation = previousLocation;

            response.Message = "Just as death seems certain, the monkey's paw in your pocket begins to tremble violently! " +
                "Its last curled finger suddenly straightens, and reality warps around you...\n\n" +
                "In a flash of eldritch light, you find yourself back where you were moments ago. " +
                "The paw lies still in your pocket, its power now spent. You've cheated death, but at what cost?\n\n" +
                (prevLocationData?.Description ?? $"You are at {previousLocation}.");

            response.ImageFilename = prevLocationData?.Image;

            return true;
        }

        private static bool TryUseLamp(ProcessCommandResponse response)
        {
            var saveData = response.saveGameData;

            // Check if player has Aladdin's lamp
            bool hasLamp = saveData.Inventory?.Any(item =>
                item.Equals("aladdinsLamp", StringComparison.OrdinalIgnoreCase) ||
                item.Equals("lamp", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasLamp) return false;

            // Check if the lamp has already been used
            if (saveData.GetEvent("lamp_used") != null)
            {
                return false;
            }

            // Use the lamp to escape!
            var previousLocation = saveData.PreviousLocation;
            var prevLocationData = WorldData.Instance?.GetLocation(previousLocation);

            saveData.AddEvent("lamp_used",
                "Used Aladdin's lamp to escape certain death",
                saveData.CurrentDateTime);

            saveData.CurrentLocation = previousLocation;

            response.Message = "In your moment of peril, you instinctively rub Aladdin's lamp! " +
                "A genie materializes in a swirl of smoke and booms: 'YOUR WISH IS MY COMMAND!'\n\n" +
                "Before you can even speak, the genie sees your predicament and whisks you away to safety. " +
                "When the smoke clears, you find yourself back where you started. " +
                "The lamp grows cold in your hands - its one wish has been granted.\n\n" +
                (prevLocationData?.Description ?? $"You are at {previousLocation}.");

            response.ImageFilename = prevLocationData?.Image;

            return true;
        }
    }
}
