using System.Text.Json.Serialization;
using Erwin.Games.TreasureIsland.Commands;

namespace Erwin.Games.TreasureIsland.Models
{
    public class Location
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string>? Items { get; set; }
        public string? Image { get; set; }
        public List<Movement>? AllowedMovements { get; set; }
        /// <summary>
        /// List of actions that automatically happen when the player enters the location
        /// </summary>
        public List<string>? Actions { get; set; }
        /// <summary>
        /// This is the list of custom commands that are allowed in this location
        /// </summary>
        public List<string>? AllowedCommands { get; set; }

        [JsonIgnore]
        public IAIClient? AiClient { get; set;}

        /// <summary>
        /// This returns a description of the location along with any items that are in the location
        /// and optionally any items dropped here by the player
        /// </summary>
        /// <returns>string description</returns>
        public async Task<string?> GetDescription(SaveGameData? saveGame = null)
        {
            var description = Description;

            var finalItemsHashSet = GetCurrentItems(saveGame);          

            if (finalItemsHashSet != null && finalItemsHashSet.Count > 0)
            {
                description += "\n\nYou see the following items:\n\n";
                foreach (var item in finalItemsHashSet)
                {
                    description += item + "\n";
                }
            }

            // what would be cool is to use the AI Client to generate a description of the location
            // which is accurate, but also has a bit of a twist to it or flavor

            if (AiClient != null && saveGame?.AiEmbelleshedDescriptions == true)
            {
                return await AiClient.GetEmbelleshedLocationDescription(description) ?? description;
            }
            return description;
        }

        public HashSet<string> GetCurrentItems(SaveGameData? saveGame)
        {
            // build a current set of items at this location
            // start with default set of items
            // remove any items that have been removed by player
            // add any items droped by user
            var locationChange = saveGame?.LocationChanges?.FirstOrDefault(l => l.Name == Name);
            var defaultItemsHashSet = new HashSet<string>(Items ?? new List<string>());
            var locationRemovedHashSet = new HashSet<string>(locationChange?.ItemsRemoved ?? new List<string>());
            var finalItemsHashSet = defaultItemsHashSet.Except(locationRemovedHashSet).ToHashSet();
            var addedItemsHashSet = new HashSet<string>(locationChange?.ItemsAdded ?? new List<string>());
            finalItemsHashSet.UnionWith(addedItemsHashSet);

            return finalItemsHashSet;
        }
    }
}