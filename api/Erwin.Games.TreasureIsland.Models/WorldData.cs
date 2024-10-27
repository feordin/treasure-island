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
    }
}