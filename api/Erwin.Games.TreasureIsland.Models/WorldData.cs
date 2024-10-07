using System;
using System.Collections.Generic;

namespace Erwin.Games.TreasureIsland.Models
{
    public class WorldData
    {
        public string? id { get; set; }
        public List<Location>? Locations { get; set; }
        public List<string>? GlobalCommands { get; set; }

        public static WorldData? Instance {get; set;}

        public Location? GetLocation(string? locationName)
        {
            return Locations?.Find(l => l.Name == locationName);
        }
    }
}