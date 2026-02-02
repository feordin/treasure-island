using System;
using System.Linq;
using Erwin.Games.TreasureIsland.Models;
using Erwin.Games.TreasureIsland.Persistence;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class FillCommand : ICommand
    {
        private readonly SaveGameData? _saveGameData;
        private readonly string? _target;

        public FillCommand(SaveGameData? saveGameData, string? target)
        {
            _saveGameData = saveGameData;
            _target = target;
        }

        public Task<ProcessCommandResponse?> Execute()
        {
            if (_saveGameData == null || WorldData.Instance == null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Unable to get the current game state or world data.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check what we're trying to fill
            if (string.IsNullOrEmpty(_target) ||
                !_target.Equals("canteen", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "What do you want to fill? Try 'fill canteen'.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if player has canteen
            bool hasCanteen = _saveGameData.Inventory?.Any(item =>
                item.Equals("canteen", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (!hasCanteen)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You don't have a canteen to fill.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if canteen is already filled
            if (_saveGameData.GetEvent("canteen_filled") != null)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "Your canteen is already full of water.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }

            // Check if in WestIceCave with melted ice
            bool inWestIceCave = _saveGameData.CurrentLocation?.Equals("WestIceCave", StringComparison.OrdinalIgnoreCase) == true;
            bool iceMelted = _saveGameData.GetEvent("ice_melted") != null;

            // Check if at a water source (creek locations)
            bool atCreek = _saveGameData.CurrentLocation?.Contains("Creek", StringComparison.OrdinalIgnoreCase) == true;

            if (inWestIceCave && iceMelted)
            {
                _saveGameData.AddEvent("canteen_filled", "Filled canteen with melted ice water", _saveGameData.CurrentDateTime);
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You fill your canteen with the cold water from the melted ice. You may need this later!",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }
            else if (atCreek)
            {
                _saveGameData.AddEvent("canteen_filled", "Filled canteen with creek water", _saveGameData.CurrentDateTime);
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "You fill your canteen with fresh water from the creek.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }
            else if (inWestIceCave && !iceMelted)
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "The ice is frozen solid. You need to melt it first to get water.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }
            else
            {
                return Task.FromResult<ProcessCommandResponse?>(new ProcessCommandResponse(
                    message: "There's no water source here to fill your canteen.",
                    saveGameData: _saveGameData,
                    imageFilename: null,
                    locationDescription: null,
                    commandHistory: null));
            }
        }
    }
}
