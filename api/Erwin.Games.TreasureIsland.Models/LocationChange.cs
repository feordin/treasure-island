namespace Erwin.Games.TreasureIsland.Models
{
    public class LocationChange
    {
        public string? Name { get; set; }
        public List<string>? ItemsAdded { get; set; }
        public List<string>? ItemsRemoved { get; set; }

        public List<string>? ThingsOpened { get; set; }
        public List<string>? ThingsClosed { get; set; }
        public DateTime? ChangeTime { get; set; }

        public LocationChange(string name, DateTime? changeTime)
        {
            Name = name;
            ItemsRemoved = new List<string>();
            ItemsAdded = new List<string>();
            ThingsClosed = new List<string>();
            ThingsOpened = new List<string>();
            ChangeTime = changeTime;
        }

        public LocationChange(string name, string item, bool added, DateTime? changeTime)
        {
            Name = name;
            ThingsClosed = new List<string>();
            ThingsOpened = new List<string>();
            ItemsRemoved = new List<string>();
            ItemsAdded = new List<string>();
            (added ? ItemsAdded : ItemsRemoved).Add(item);
            ChangeTime = changeTime;
        }
    }
}