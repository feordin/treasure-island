using System.Text.Json.Serialization;
using Erwin.Games.TreasureIsland.Commands;

namespace Erwin.Games.TreasureIsland.Models
{
    public class Item
    {
        public string? Name { get; set; }
        /// <summary>
        /// User-friendly display name shown to the player. Falls back to Name if not set.
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// Gets the display name, falling back to Name if DisplayName is not set.
        /// </summary>
        public string GetDisplayName() => DisplayName ?? Name ?? "Unknown Item";
        public string? Description { get; set; }
        public string? ExamineText { get; set; }

        public bool? Takeable { get; set; }

        public bool? MustBuy { get; set; }
        public int? Cost { get; set; }
        public int? Value { get; set; }
        public string? Reveals {get; set;}

        public bool IsTakeable
        {
            get
            {
                return Takeable == null ? true : Takeable.Value;
            }
        }

        public bool IsMustBuy
        {
            get
            {
                return MustBuy == null ? false : MustBuy.Value;
            }
        }
    }
}