using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Actions
{
    /// <summary>
    /// Global Dracula action that runs on every command.
    /// Day: Dracula sleeps in CoffinRoom. If player is there, show sleeping text + coffin image.
    /// Night: Dracula wanders from location to location. If same location as player, attack!
    /// Dawn: Reset Dracula to CoffinRoom.
    /// </summary>
    public class DraculaAction : IAction
    {
        private readonly ProcessCommandResponse _response;
        private static readonly Random _random = new Random();

        public DraculaAction(ProcessCommandResponse response)
        {
            _response = response;
        }

        public void Execute()
        {
            if (_response?.saveGameData == null || WorldData.Instance == null)
                return;

            // If Dracula has been killed, do nothing
            if (_response.saveGameData.GetEvent("killed_dracula") != null)
            {
                // If player is in CoffinRoom, mention the empty coffin
                if (_response.saveGameData.CurrentLocation?.Equals("CoffinRoom", StringComparison.OrdinalIgnoreCase) == true)
                {
                    _response.Message += "\n\nThe coffin is empty. Only a pile of dust remains where Dracula once lay.";
                }
                return;
            }

            var currentHour = _response.saveGameData.CurrentDateTime.Hour;
            bool isNight = currentHour >= 20 || currentHour < 6;

            string draculaLocation = GetDraculaLocation();

            if (isNight)
            {
                HandleNight(draculaLocation);
            }
            else
            {
                HandleDay(draculaLocation);
            }
        }

        private string GetDraculaLocation()
        {
            var locationEvent = _response.saveGameData.GetEvent("dracula_location");
            return locationEvent?.Description ?? "CoffinRoom";
        }

        private void SetDraculaLocation(string location)
        {
            _response.saveGameData.RemoveEvent("dracula_location");
            _response.saveGameData.AddEvent("dracula_location", location, _response.saveGameData.CurrentDateTime);
        }

        private void HandleDay(string draculaLocation)
        {
            // At dawn, reset Dracula to CoffinRoom if he was wandering
            if (!draculaLocation.Equals("CoffinRoom", StringComparison.OrdinalIgnoreCase))
            {
                SetDraculaLocation("CoffinRoom");
                draculaLocation = "CoffinRoom";
            }

            // If player is in CoffinRoom during the day, show Dracula sleeping
            if (_response.saveGameData.CurrentLocation?.Equals("CoffinRoom", StringComparison.OrdinalIgnoreCase) == true)
            {
                _response.Message += "\n\nYou cautiously approach the ornate coffin. Inside, you see the infamous Count Dracula lying motionless, pale as death. He appears to be in a deep slumber, vulnerable during the daylight hours.";
                _response.ImageFilename = "draculasleeping.png";
            }
        }

        private void HandleNight(string draculaLocation)
        {
            // Ensure Dracula starts at CoffinRoom if no location is set
            if (string.IsNullOrEmpty(draculaLocation))
            {
                draculaLocation = "CoffinRoom";
                SetDraculaLocation(draculaLocation);
            }

            // Move Dracula to a random adjacent location
            var currentDraculaLoc = WorldData.Instance?.GetLocation(draculaLocation);
            if (currentDraculaLoc?.AllowedMovements != null && currentDraculaLoc.AllowedMovements.Count > 0)
            {
                var destinations = currentDraculaLoc.AllowedMovements
                    .Where(m => m.Destination != null)
                    .Select(m => m.Destination!)
                    .ToList();

                if (destinations.Count > 0)
                {
                    var newLocation = destinations[_random.Next(destinations.Count)];
                    SetDraculaLocation(newLocation);
                    draculaLocation = newLocation;
                }
            }

            // Check if Dracula is at the same location as the player
            if (draculaLocation.Equals(_response.saveGameData.CurrentLocation, StringComparison.OrdinalIgnoreCase))
            {
                // Track previous location for last-chance escape
                _response.saveGameData.PreviousLocation = _response.saveGameData.CurrentLocation;

                // Try last-chance escape before death
                if (LastChanceEscape.TryEscape(_response, "Dracula attack"))
                {
                    return;
                }

                // Dracula kills the player
                _response.saveGameData.AddEvent("GameOver", "Killed by Dracula", _response.saveGameData.CurrentDateTime);
                _response.Message += "\n\nA shadow falls over you. You turn to see Count Dracula looming behind you, his eyes glowing crimson red in the darkness! His pale hands reach for your throat with inhuman speed. Your adventure ends here in the darkness...";
                _response.ImageFilename = "draculaface.png";
            }
        }
    }
}
