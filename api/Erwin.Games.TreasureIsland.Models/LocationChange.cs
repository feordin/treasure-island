namespace Erwin.Games.TreasureIsland.Models
{
    public class LocationChange
    {
        public string? Name { get; set; }
        public List<string>? ItemsAdded { get; set; }
        public List<string>? ItemsRemoved { get; set; }

        public List<string>? ThingsOpened { get; set; }
        public List<string>? ThingsClosed { get; set; }
        public DateTime ChangeTime { get; set; }
    }
}