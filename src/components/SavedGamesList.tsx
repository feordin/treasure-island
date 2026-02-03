// src/components/SavedGamesList.tsx
import React from 'react';
import './SavedGamesList.css';
import { SaveGameData } from '../modules/ProcessCommandResponse';

interface SavedGamesListProps {
  savedGames: SaveGameData[];
}

const SavedGamesList: React.FC<SavedGamesListProps> = ({ savedGames }) => {
  return (
    <div className="saved-games-list">
      <h2>Saved Games</h2>
      <table>
        <thead>
          <tr>
            <th>Number</th>
            <th>Date</th>
            <th>Time</th>
            <th>Location</th>
          </tr>
        </thead>
        <tbody>
          {savedGames.map((game, index) => (
            <tr key={index}>
              <td>{index}</td>
              <td>{new Date(game.currentDateTime || "01-01-1789").toLocaleDateString('en-US', { timeZone: 'UTC' })}</td>
              <td>{new Date(game.currentDateTime || "12:00").toLocaleTimeString('en-US', { timeZone: 'UTC' })}</td>
              <td>{game.currentLocationDisplayName || game.currentLocation}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default SavedGamesList;