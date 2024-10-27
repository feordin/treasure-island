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
        public List<Event>? Events { get; set; }
        public int? Money { get; set; } = 0;
        public string? Facing { get; set; } = "south";

        public Event? GetEvent(string? eventName)
        {
            return Events?.Find(e => string.Equals(e.Name, eventName, StringComparison.OrdinalIgnoreCase) == true);
        }

        public bool AddEvent(string? eventName, string? eventDescription, DateTime? eventDate)
        {
            if (Events == null)
            {
                Events = new List<Event>();
            }

            if (Events.Exists(e => string.Equals(e.Name, eventName, StringComparison.OrdinalIgnoreCase) == true))
            {
                return false;
            }

            Events.Add(new Event
            {
                Name = eventName,
                Description = eventDescription,
                EventDate = eventDate
            });

            return true;
        }

        public bool RemoveEvent(string? eventName)
        {
            if (Events == null)
            {
                return false;
            }

            int removedCount = Events.RemoveAll(e => string.Equals(e.Name, eventName, StringComparison.OrdinalIgnoreCase));

            return removedCount > 0;
        }
    }
}