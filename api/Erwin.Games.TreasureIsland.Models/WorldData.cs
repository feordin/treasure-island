using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;

namespace Erwin.Games.TreasureIsland.Models
{
    public class WorldData
    {
        public string? id { get; set; }
        public List<Location>? Locations { get; set; }
        public List<Item>? Items { get; set; }
        public List<string>? GlobalCommands { get; set; }
        public string? IntroText { get; set; }

    public static WorldData? Instance { get; set; }

        public Location? GetLocation(string? locationName)
        {
            return Locations?.Find(l => l.Name == locationName);
        }

        public Item? GetItem(string? itemName)
        {
            return Items?.Find(i => i.Name?.Equals(itemName, StringComparison.OrdinalIgnoreCase) == true);
        }

        /// <summary>
        /// Gets the display name for an item, falling back to the system name.
        /// </summary>
        public string GetItemDisplayName(string? itemName)
        {
            var item = GetItem(itemName);
            return item?.GetDisplayName() ?? itemName ?? "Unknown Item";
        }

        /// <summary>
        /// Resolves player input to the correct system item name using fuzzy matching.
        /// Tries: exact match, substring, reverse substring, display name match.
        /// </summary>
        public string? ResolveItemName(string? input, IEnumerable<string>? validItems)
        {
            if (string.IsNullOrEmpty(input) || validItems == null)
                return input;

            var inputLower = input.ToLowerInvariant();
            var validList = validItems.ToList();

            // 1. Exact match (case-insensitive)
            var exact = validList.FirstOrDefault(v => v.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            // 2. Alphanumeric-only match (strips apostrophes, punctuation)
            // e.g., "monkey'sPaw" matches "monkeysPaw"
            var inputAlpha = AlphaOnly(inputLower);
            var alphaMatch = validList.FirstOrDefault(v => AlphaOnly(v.ToLowerInvariant()) == inputAlpha);
            if (alphaMatch != null) return alphaMatch;

            // 3. Substring match - valid item name contains input
            var substringMatch = validList.FirstOrDefault(v => v.Contains(input, StringComparison.OrdinalIgnoreCase));
            if (substringMatch != null) return substringMatch;

            // 4. Reverse substring - input contains valid item name
            var reverseMatch = validList.FirstOrDefault(v => input.Contains(v, StringComparison.OrdinalIgnoreCase));
            if (reverseMatch != null) return reverseMatch;

            // 5. Display name match - normalize both to stripped lowercase (no spaces or punctuation)
            var inputNormalized = AlphaOnly(inputLower);
            foreach (var validItem in validList)
            {
                var item = GetItem(validItem);
                if (item?.DisplayName != null)
                {
                    var displayNormalized = AlphaOnly(item.DisplayName.ToLowerInvariant());
                    if (displayNormalized == inputNormalized || displayNormalized.Contains(inputNormalized) || inputNormalized.Contains(displayNormalized))
                        return validItem;
                }
            }

            // No match found - return original input for error messages
            return input;
        }

        /// <summary>
        /// Strips everything except letters and digits for fuzzy matching.
        /// e.g., "monkey'sPaw" → "monkeyspaw", "King Tut's Treasure" → "kingtutstreasure"
        /// </summary>
        private static string AlphaOnly(string input)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var c in input)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}