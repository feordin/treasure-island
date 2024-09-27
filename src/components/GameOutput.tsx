// src/components/GameOutput.tsx
import React, { useEffect, useRef } from 'react';
import './GameOutput.css';

interface Message {
  sender: 'user' | 'game';
  text: string;
}

interface GameOutputProps {
  messages: Message[];
}

const GameOutput: React.FC<GameOutputProps> = ({ messages }) => {
    const messagesEndRef = useRef<HTMLDivElement>(null);
  
    useEffect(() => {
      messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);
  
    return (
      <div className="game-output">
        {messages.map((message, index) => (
          <div key={index} className={`message ${message.sender}`}>
            {message.text}
          </div>
        ))}
        <div ref={messagesEndRef} />
      </div>
    );
  };

export default GameOutput;
