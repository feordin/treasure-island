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
    }
}