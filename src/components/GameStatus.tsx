import React from 'react';
import './GameStatus.css';

interface GameStatusProps {
  inventory: string[];
  score: number;
  location: string;
  date: string;
  time: string;
  player: string;
  currentLocationDescription: string;
}

const GameStatus: React.FC<GameStatusProps> = ({ inventory, score, location, date, time, player, currentLocationDescription }) => {
  return (
    <div className="game-status">
      <h2>Game Status</h2>
      <div className="status-item">
        <strong>Player:</strong> {player}
      </div>
      <div className="status-item">
        <strong>Score:</strong> {score}
      </div>
      <div className="status-item">
        <strong>Location:</strong> {location}
      </div>
      <div className="status-item">
        <strong>Date:</strong> {date}
      </div>
      <div className="status-item">
        <strong>Time:</strong> {time}
      </div>
      <div className="inventory">
        <strong>Inventory:</strong>
        <div className="inventory-list-container">
            <ul>
                {inventory.map((item, index) => (
                <li key={index}>{item}</li>
                ))}
            </ul>
        </div>
      </div>
      <div className="status-item">
        <strong>Location Description:</strong> {currentLocationDescription} {/* New text block */}
      </div>
    </div>
  );
};

export default GameStatus;