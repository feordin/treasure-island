using System.Drawing;
using System.Text;
using System.Text.Json.Serialization;
using Erwin.Games.TreasureIsland.Commands;

namespace Erwin.Games.TreasureIsland.Models
{
    public class Location
    {
        public string? Name { get; set; }
        /// <summary>
        /// User-friendly display name shown to the player. Falls back to Name if not set.
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// Gets the display name, falling back to Name if DisplayName is not set.
        /// </summary>
        public string GetDisplayName() => DisplayName ?? Name ?? "Unknown Location";
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
        public IAIClient? AiClient { get; set; }

        public string? Help { get; set; }

        /// <summary>
        /// Point cost for receiving the help hint at this location. If > 0, player pays this cost once.
        /// </summary>
        public int HelpCost { get; set; } = 0;

        public static string? CardinalDirectionToSimple(string? facing, string? cardinalDirection)
        {
            facing = facing?.ToLower();
            var direction = cardinalDirection?.ToLower();

            if (facing == "north")
            {
                return direction switch
                {
                    "north" => "ahead",
                    "south" => "behind",
                    "east" => "right",
                    "west" => "left",
                    _ => direction
                };
            }
            else if (facing == "south")
            {
                return direction switch
                {
                    "north" => "behind",
                    "south" => "ahead",
                    "east" => "left",
                    "west" => "right",
                    _ => direction
                };
            }
            else if (facing == "east")
            {
                return direction switch
                {
                    "north" => "left",
                    "south" => "right",
                    "east" => "ahead",
                    "west" => "behind",
                    _ => direction
                };
            }
            else if (facing == "west")
            {
                return direction switch
                {
                    "north" => "right",
                    "south" => "left",
                    "east" => "behind",
                    "west" => "ahead",
                    _ => direction
                };
            }

            return direction;
        }

        public static string? SimpleToCardinalDirection(string? facing, string? simpleDirection)
        {
            if (string.IsNullOrEmpty(facing) || string.IsNullOrEmpty(simpleDirection))
            {
                return simpleDirection;
            }

            facing = facing.ToLower();
            simpleDirection = simpleDirection.ToLower();

            if (facing == "north")
            {
                return simpleDirection switch
                {
                    "ahead" => "north",
                    "behind" => "south",
                    "right" => "east",
                    "left" => "west",
                    _ => simpleDirection
                };
            }
            else if (facing == "south")
            {
                return simpleDirection switch
                {
                    "ahead" => "south",
                    "behind" => "north",
                    "right" => "west",
                    "left" => "east",
                    _ => simpleDirection
                };
            }
            else if (facing == "east")
            {
                return simpleDirection switch
                {
                    "ahead" => "east",
                    "behind" => "west",
                    "right" => "south",
                    "left" => "north",
                    _ => simpleDirection
                };
            }
            else if (facing == "west")
            {
                return simpleDirection switch
                {
                    "ahead" => "west",
                    "behind" => "east",
                    "right" => "north",
                    "left" => "south",
                    _ => simpleDirection
                };
            }

            return simpleDirection;
        }

        /// <summary>
        /// This returns a description of the location along with any items that are in the location
        /// and optionally any items dropped here by the player
        /// </summary>
        /// <returns>string description</returns>
        public async Task<string?> GetDescription(SaveGameData? saveGame = null)
        {
            var description = Description;

            // Add direction information from allowed movements property
            if (AllowedMovements != null && AllowedMovements.Count > 0)
            {
                foreach (var movement in AllowedMovements)
                {
                    var translatedDirection = Location.CardinalDirectionToSimple(saveGame?.Facing, movement?.Direction?[0]);
                    // Get the display name for the destination location
                    var destinationLocation = WorldData.Instance?.GetLocation(movement?.Destination);
                    var destinationName = destinationLocation?.GetDisplayName() ?? movement?.Destination;

                    if (translatedDirection == "right" || translatedDirection == "left")
                        description += $"\nOn the {translatedDirection} ({movement?.Direction?[0]}) is {destinationName}.";
                    else if (translatedDirection == "ahead")
                        description += $"\nAhead of you ({saveGame?.Facing}) is {destinationName}.";
                    else if (translatedDirection == "up")
                        description += $"\nUp from here is {destinationName}.";
                    else if (translatedDirection == "down")
                        description += $"\nDown from here is {destinationName}.";
                    else
                        description += $"\nBehind you is {destinationName}.";
                }
            }


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
            var stringComparer = StringComparer.OrdinalIgnoreCase;

            var locationChange = saveGame?.LocationChanges?.FirstOrDefault(l => l.Name == Name);
            var defaultItemsHashSet = new HashSet<string>(Items ?? new List<string>(), stringComparer);
            var locationRemovedHashSet = new HashSet<string>(locationChange?.ItemsRemoved ?? new List<string>(), stringComparer);
            var finalItemsHashSet = defaultItemsHashSet.Except(locationRemovedHashSet).ToHashSet();
            var addedItemsHashSet = new HashSet<string>(locationChange?.ItemsAdded ?? new List<string>(), stringComparer);
            finalItemsHashSet.UnionWith(addedItemsHashSet);

            return finalItemsHashSet;
        }

        public bool AddItemToLocation(SaveGameData? saveGame, string? item)
        {
            if (saveGame == null || item == null)
            {
                return false;
            }

            var locationChange = saveGame.LocationChanges?.FirstOrDefault(l => l.Name == Name);
            if (locationChange == null)
            {
                if (Name == null)
                {
                    return false;
                }
                locationChange = new LocationChange(Name, item, true, saveGame.CurrentDateTime);
                saveGame.LocationChanges?.Add(locationChange);
            }
            else
            {
                if (locationChange.ItemsAdded?.Contains(item, StringComparer.OrdinalIgnoreCase) == true)
                {
                    return false;
                }
                locationChange.ItemsAdded?.Add(item);
            }

            return true;
        }

        public bool RemoveItemFromLocation(SaveGameData? saveGame, string? item)
        {
            if (saveGame == null || item == null)
            {
                return false;
            }

            var locationChange = saveGame.LocationChanges?.FirstOrDefault(l => l.Name == Name);
            if (locationChange == null)
            {
                if (Name == null)
                {
                    return false;
                }
                locationChange = new LocationChange(Name, item, false, saveGame.CurrentDateTime);
                saveGame.LocationChanges?.Add(locationChange);
            }
            else
            {
                if (locationChange.ItemsRemoved?.Contains(item, StringComparer.OrdinalIgnoreCase) == true)
                {
                    return false;
                }

                if (locationChange.ItemsAdded?.Contains(item, StringComparer.OrdinalIgnoreCase) == true)
                {
                    locationChange.ItemsAdded?.RemoveAt(locationChange.ItemsAdded.FindIndex(n => n.Equals(item, StringComparison.OrdinalIgnoreCase)));
                }
                else
                {
                    locationChange.ItemsRemoved?.Add(item);
                }
            }

            return true;
        }

        public async Task<string?> GetFortune()
        {
            if (AiClient != null)
                return await AiClient.GetFortune();
            else
                return "You see a crystal ball, but no one is here to read your fortune.";
        }

        public async Task<string?> Pray()
        {
            if (AiClient != null)
                return await AiClient.Pray();
            else
                return "The heavens seem closed at the moment.  God answers prayers in his own time.";
        }

        public bool AddMovementToLocation(SaveGameData? saveGame, Movement movement)
        {
            if (saveGame == null || movement == null)
            {
                return false;
            }

            var locationChange = saveGame.LocationChanges?.FirstOrDefault(l => l.Name == Name);
            if (locationChange == null)
            {
                if (Name == null)
                {
                    return false;
                }
                locationChange = new LocationChange(Name, saveGame.CurrentDateTime);
                saveGame.LocationChanges?.Add(locationChange);
            }

            if (locationChange.MovementsAdded?.Any(m => m.Direction?.Any(d => movement.Direction?.Contains(d, StringComparer.OrdinalIgnoreCase) == true) == true) == true)
            {
                return false;
            }

            locationChange.MovementsAdded?.Add(movement);
            return true;
        }

        public bool RemoveMovementFromLocation(SaveGameData? saveGame, string? direction)
        {
            if (saveGame == null || direction == null)
            {
                return false;
            }

            var locationChange = saveGame.LocationChanges?.FirstOrDefault(l => l.Name == Name);
            if (locationChange == null)
            {
                if (Name == null)
                {
                    return false;
                }
                locationChange = new LocationChange(Name, saveGame.CurrentDateTime);
                saveGame.LocationChanges?.Add(locationChange);
            }

            if (locationChange.MovementsRemoved?.Contains(direction, StringComparer.OrdinalIgnoreCase) == true)
            {
                return false;
            }

            locationChange.MovementsRemoved?.Add(direction);
            return true;
        }

        public List<Movement> GetCurrentMovements(SaveGameData? saveGame)
        {
            var stringComparer = StringComparer.OrdinalIgnoreCase;
            var locationChange = saveGame?.LocationChanges?.FirstOrDefault(l => l.Name == Name);

            // Start with base movements
            var currentMovements = new List<Movement>(AllowedMovements ?? new List<Movement>());

            // Remove blocked movements
            if (locationChange?.MovementsRemoved != null)
            {
                currentMovements = currentMovements
                    .Where(m => !locationChange.MovementsRemoved.Any(removed =>
                        m.Direction?.Contains(removed, stringComparer) == true))
                    .ToList();
            }

            // Add dynamic movements
            if (locationChange?.MovementsAdded != null)
            {
                currentMovements.AddRange(locationChange.MovementsAdded);
            }

            return currentMovements;
        }
    }
}