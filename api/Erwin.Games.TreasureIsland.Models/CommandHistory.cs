using System;

namespace Erwin.Games.TreasureIsland.Models
{
    public class CommandHistory
    {
        public List<string>? Command { get; set; }
        public List<string>? Response { get; set; }
        public string id { get; set; } = string.Empty;

        public void AddHistory(string? command, string? response)
        {
            if (Command == null && Response == null)
            {
                Command = new List<string>();
                Response = new List<string>();
            }

            if (command != null && Command != null)
                Command.Add(command);

            if (response != null && Response != null)
                Response.Add(response);
        }
    }
}