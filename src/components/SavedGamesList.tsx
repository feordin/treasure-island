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
      <h3>Saved Games</h3>
      <ul>
        {savedGames.map((game, index) => (
          <li key={index}>
            <p>Date: {new Date(game.currentDateTime || "01-01-1789").toLocaleDateString()}</p>
            <p>Time: {new Date(game.currentDateTime || "12:00").toLocaleTimeString()}</p>
            <p>Location: {game.currentLocation}</p>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default SavedGamesList;