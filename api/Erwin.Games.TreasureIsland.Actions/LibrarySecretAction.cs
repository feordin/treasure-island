using System;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    public class LibrarySecretAction : IAction
    {
        private readonly ProcessCommandResponse _response;

        public LibrarySecretAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            // Check if the secret has been opened
            if (_response.saveGameData.GetEvent("library_secret_opened") != null)
            {
                // Ensure west movement to HiddenRoom is added (in case of save/load)
                var library = WorldData.Instance?.GetLocation("Library");
                if (library != null)
                {
                    var westMovement = new Movement
                    {
                        Direction = new[] { "west" },
                        Destination = "HiddenRoom",
                        TimeToMove = 1
                    };
                    library.AddMovementToLocation(_response.saveGameData, westMovement);
                }

                _response.Message += "\n\nThe bookshelf stands open, revealing a passage to the west.";
            }
        }
    }
}
