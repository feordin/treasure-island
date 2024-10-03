// src/App.tsx
import React, { useState, useEffect } from 'react';
import './App.css';
import GameOutput from './components/GameOutput';
import GameInput from './components/GameInput';
import LocationImage from './components/LocationImage';
import GameStatus from './components/GameStatus';
import { ProcessCommandResponse, CommandRequest, SaveGameData } from './modules/ProcessCommandResponse';


interface Message {
  sender: 'user' | 'game';
  text: string;
}

var inventory: string[] = [];
var score = 100;
var location = 'Foo';
var date = new Date().toLocaleDateString();
var time = new Date().toLocaleTimeString();
var currentLocationDescription = 'Starting location';

function App() {
  const [messages, setMessages] = useState<Message[]>([
    { sender: 'game', text: 'Welcome to your adventure!' },
  ]);

  const [currentLocationImage, setCurrentLocationImage] = useState<string>('images/hilltop.png'); // Initial image

  var currentGame: SaveGameData | undefined = {
    player: "Test",
    score: 0,
    currentLocation: "Foo",
    currentDateTime: "1978-01-01",
    inventory: [],
    health: 0,
    history: undefined,
    locationChanges: []
  };

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
    if (newImage && newImage !== currentLocationImage && newImage !== '' && newImage !== undefined) {
      console.log('Update image:', newImage);
      setCurrentLocationImage(newImage);
    }

    // update the game status
    if (currentGame) {
      if (currentGame.currentLocation && location !== currentGame.currentLocation) {
        location = currentGame.currentLocation;
        currentLocationDescription = response;
      }
      if (currentGame.currentDateTime) {
        date = new Date(currentGame.currentDateTime).toLocaleDateString();
        time = new Date(currentGame.currentDateTime).toLocaleTimeString();
      }
      if (currentGame.inventory) {
        inventory = currentGame.inventory;
      }
      if (currentGame.score) {
        score = currentGame.score;
      }
    }
  };

  // invoke the azure function here
  const invokeAzureFunction = async (command: CommandRequest) => {
    try {
    const response = await fetch(process.env.REACT_APP_API_URL + '/api/ProcessGameCommand', {
      method: 'POST',
      headers: {
      'Content-Type': 'application/json',
      },
      body: JSON.stringify(command),
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
    
    const commandRequest: CommandRequest = {
      command: command,
      saveGameData: currentGame
    }

    const azureResponse = await invokeAzureFunction(commandRequest);
    console.log('Azure response:', azureResponse);
    const parsedResponse: ProcessCommandResponse = typeof azureResponse === 'string' ? JSON.parse(azureResponse) : azureResponse;
    currentGame = parsedResponse.saveGameData;
    return {
      response: parsedResponse.message || 'No response from server',
      newImage: parsedResponse.image
    };
  };

  useEffect(() => {
    handleCommandSubmit("init");
  }, []); // Empty de

  return (
    <div className="App">
      <GameOutput messages={messages} />
      <GameInput onCommandSubmit={handleCommandSubmit} />
      <LocationImage imageSrc={currentLocationImage} />
      <GameStatus inventory={inventory} score={score} location={location} date={date} time={time} currentLocationDescription={currentLocationDescription} />
    </div>
  );
}

export default App;
