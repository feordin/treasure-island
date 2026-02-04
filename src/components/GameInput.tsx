// src/components/GameInput.tsx
import React, { useState } from 'react';
import './GameInput.css';
import { speechToText, isSpeechSupported } from '../services/speechService';

interface GameInputProps {
  onCommandSubmit: (command: string) => void;
}

const GameInput: React.FC<GameInputProps> = ({ onCommandSubmit }) => {
  const [command, setCommand] = useState('');
  const [isListening, setIsListening] = useState(false);
  const [micError, setMicError] = useState<string | null>(null);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (command.trim() !== '') {
      onCommandSubmit(command);
      setCommand('');
    }
  };

  const handleMicClick = async () => {
    if (isListening) return;

    setIsListening(true);
    setMicError(null);

    try {
      const text = await speechToText();
      if (text) {
        setCommand(text);
        // Auto-submit the voice command
        onCommandSubmit(text);
        setCommand('');
      }
    } catch (error) {
      console.error('Speech recognition error:', error);
      setMicError(error instanceof Error ? error.message : 'Speech recognition failed');
      // Clear error after 3 seconds
      setTimeout(() => setMicError(null), 3000);
    } finally {
      setIsListening(false);
    }
  };

  const showMicButton = isSpeechSupported();

  return (
    <div className="game-input">
      <form onSubmit={handleSubmit}>
        {showMicButton && (
          <button
            type="button"
            className={`mic-button ${isListening ? 'listening' : ''}`}
            onClick={handleMicClick}
            disabled={isListening}
            title={isListening ? 'Listening...' : 'Click to speak'}
          >
            {isListening ? '...' : '🎤'}
          </button>
        )}
        <input
          type="text"
          value={command}
          onChange={(e) => setCommand(e.target.value)}
          placeholder={isListening ? 'Listening...' : 'Enter command...'}
          disabled={isListening}
        />
        <button type="submit" disabled={isListening}>Send</button>
      </form>
      {micError && <div className="mic-error">{micError}</div>}
    </div>
  );
};

export default GameInput;
