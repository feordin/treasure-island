namespace Erwin.Games.TreasureIsland.Models
{
    public class Movement
    {
        public string? Direction { get; set; }
        public string? Destination { get; set; }
        public TimeSpan? TimeToMove { get; set; }
    }
}