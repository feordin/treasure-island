using System;
using System.Collections.Generic;

namespace Erwin.Games.TreasureIsland.Models
{
    public class SaveGameData
    {
        public string? id { get; set; }
        public string? Player { get; set; }
        public int Score { get; set; }
        public string? CurrentLocation { get; set; }
        public DateTime CurrentDateTime { get; set; }
        public List<string>? Inventory { get; set; }
        public int Health { get; set; }
        public List<LocationChange>? LocationChanges { get; set; }
        public bool AiEmbelleshedDescriptions { get; set; } = false;
    }
}