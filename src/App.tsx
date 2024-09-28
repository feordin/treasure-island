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

  const handleCommandSubmit = async (command: string) => {
    // Add user's command to messages
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: 'user', text: command },
    ]);

    // Implement game logic here
    const { response, newImage } = await processCommand(command);

    // Add game response to messages
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: 'game', text: response },
    ]);

    // Update the location image if it has changed
    if (newImage) {
      console.log('Update image:', newImage);
      setCurrentLocationImage(newImage);
    }
  };

  // invoke the azure function here
  const invokeAzureFunction = async (command: string) => {
    try {
    const response = await fetch(process.env.REACT_APP_API_URL + '/api/ProcessGameCommand', {
      method: 'POST',
      headers: {
      'Content-Type': 'application/json',
      },
      body: JSON.stringify({ command }),
    });

    if (!response.ok) {
      throw new Error('Network response was not ok');
    }

    const data = await response.text();
    return data;
    } catch (error) {
      console.error('Error invoking Azure function:', error);
      return { response: 'There was an error processing your command.', newImage: undefined };
    }
  };

  // Example game logic function
  const processCommand = async (command: string): Promise<{ response: string; newImage?: string }> => {
    // Your game logic here
    // For demonstration, let's simulate moving to a new location when the user types "go north"
    const azureResponse = await invokeAzureFunction(command);
    console.log('Azure response:', azureResponse);

    if (command.toLowerCase() === 'go north') {
      return {
        response: 'You head north into the dark forest.',
        newImage: 'images/forest-location.png',
      };
    }

    // Default response
    return { response: `You performed the action: ${command} + ` +  azureResponse};
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
