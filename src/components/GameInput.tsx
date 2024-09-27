// src/components/GameInput.tsx
import React, { useState } from 'react';
import './GameInput.css';

interface GameInputProps {
  onCommandSubmit: (command: string) => void;
}

const GameInput: React.FC<GameInputProps> = ({ onCommandSubmit }) => {
  const [command, setCommand] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (command.trim() !== '') {
      onCommandSubmit(command);
      setCommand('');
    }
  };

  return (
    <div className="game-input">
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          value={command}
          onChange={(e) => setCommand(e.target.value)}
          placeholder="Enter command..."
        />
        <button type="submit">Send</button>
      </form>
    </div>
  );
};

export default GameInput;
