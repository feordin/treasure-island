using System.Text.Json.Serialization;
using Erwin.Games.TreasureIsland.Commands;

namespace Erwin.Games.TreasureIsland.Models
{
    public class Item
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}