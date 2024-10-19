using System.Text.Json.Serialization;
using Erwin.Games.TreasureIsland.Commands;

namespace Erwin.Games.TreasureIsland.Models
{
    public class Event
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? EventDate { get; set; }
    }
}