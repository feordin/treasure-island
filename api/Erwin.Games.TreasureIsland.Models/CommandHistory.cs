using System;

namespace Erwin.Games.TreasureIsland.Models
{
    public class CommandHistory
    {
        public List<string>? Command { get; set; }
        public List<string>? Response { get; set; }

        public void AddHistory(string command, string response)
        {
            if (Command != null && Response != null)
            {
                Command.Add(command);
                Response.Add(response);
            }
        }
    }
}