// src/App.tsx
import React, { useState } from 'react';
import './App.css';
import GameOutput from './components/GameOutput';
import GameInput from './components/GameInput';
import LocationImage from './components/LocationImage';
import GameStatus from './components/GameStatus';


interface Message {
  sender: 'user' | 'game';
  text: string;
}

const inventory = ['Sword', 'Shield', 'Potion', 'donuts', 'map', 'key', 'candle'];
const score = 100;
const location = 'Castle';
const date = new Date().toLocaleDateString();
const time = new Date().toLocaleTimeString();
const player = 'Player1';

function App() {
  const [messages, setMessages] = useState<Message[]>([
    { sender: 'game', text: 'Welcome to your adventure!' },
  ]);

  const [currentLocationImage, setCurrentLocationImage] = useState<string>('images/start-point.png'); // Initial image

  const handleCommandSubmit = (command: string) => {
    // Add user's command to messages
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: 'user', text: command },
    ]);

    // Implement game logic here
    const { response, newImage } = processCommand(command);

    // Add game response to messages
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: 'game', text: response },
    ]);

    // Update the location image if it has changed
    if (newImage) {
      setCurrentLocationImage(newImage);
    }
  };

  // Example game logic function
  const processCommand = (command: string): { response: string; newImage?: string } => {
    // Your game logic here
    // For demonstration, let's simulate moving to a new location when the user types "go north"
    if (command.toLowerCase() === 'go north') {
      return {
        response: 'You head north into the dark forest.',
        newImage: 'images/forest-location.jpg',
      };
    }

    // Default response
    return { response: `You performed the action: ${command}` };
  };

  return (
    <div className="App">
      <GameOutput messages={messages} />
      <GameInput onCommandSubmit={handleCommandSubmit} />
      <LocationImage imageSrc={currentLocationImage} />
      <GameStatus inventory={inventory} score={score} location={location} date={date} time={time} player={player} currentLocationDescription="Starting location" />
    </div>
  );
}

export default App;
