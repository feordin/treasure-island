// src/App.tsx
import React, { useState, useEffect, useRef, useCallback } from 'react';
import './App.css';
import GameOutput from './components/GameOutput';
import GameInput from './components/GameInput';
import LocationImage from './components/LocationImage';
import GameStatus from './components/GameStatus';
import { ProcessCommandResponse, CommandRequest, SaveGameData } from './modules/ProcessCommandResponse';
import { invokeAzureFunction } from './services/invokeApi';


interface Message {
  sender: 'user' | 'game';
  text: string;
}

var inventory: string[] = [];
var score: number | undefined = 100;
var location = 'Foo';
var date = new Date().toLocaleDateString();
var time = new Date().toLocaleTimeString();
var currentLocationDescription = 'Starting location';

function App() {

  const [messages, setMessages] = useState<Message[]>([
    { sender: 'game', text: 'Welcome to your adventure!' },
  ]);

  const initRef = useRef(false); // Ref to track initialization
  const [currentLocationImage, setCurrentLocationImage] = useState<string>('images/foo.png'); // Initial image

  const currentGameRef = useRef<SaveGameData | undefined>({
    player: "Test",
    score: 100,
    currentLocation: "Foo",
    currentDateTime: "1978-01-01",
    inventory: [],
    health: 0,
    history: undefined,
    locationChanges: []
  });

  const handleCommandSubmit = useCallback(async (command: string) => {
    // Add user's command to messages
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: 'user', text: command },
    ]);

    const processCommand = async (command: string): Promise<{ response: string; newImage?: string }> => {
      const commandRequest: CommandRequest = {
        command: command,
        saveGameData: currentGameRef.current
      }
      const azureResponse = await invokeAzureFunction(commandRequest);
      console.log('Azure response:', azureResponse);
      const parsedResponse: ProcessCommandResponse = typeof azureResponse === 'string' ? JSON.parse(azureResponse) : azureResponse;
      currentGameRef.current = parsedResponse.saveGameData;
      return {
        response: parsedResponse.message || 'No response from server',
        newImage: parsedResponse.imageFilename
      };
    };

    // Implement game logic here
    var { response, newImage } = await processCommand(command);

    // Add game response to messages
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: 'game', text: response },
    ]);

    newImage = "images/" + newImage;

    // Update the location image if it has changed
    if (newImage && newImage !== currentLocationImage && newImage !== '' && newImage !== undefined) {
      console.log('Update image:', newImage);
      setCurrentLocationImage(newImage);
    }

    console.log('Current game:', currentGameRef.current);
    // update the game status
    if (currentGameRef.current) {
      if (currentGameRef.current.currentLocation && location !== currentGameRef.current.currentLocation) {
        location = currentGameRef.current.currentLocation;
        currentLocationDescription = response;
      }
      if (currentGameRef.current.currentDateTime) {
        date = new Date(currentGameRef.current.currentDateTime).toLocaleDateString();
        time = new Date(currentGameRef.current.currentDateTime).toLocaleTimeString();
      }
      if (currentGameRef.current.inventory) {
        inventory = currentGameRef.current.inventory;
      }
      if (currentGameRef.current) {
        score = currentGameRef.current.score;
      }
    }

  }, [currentLocationImage]);

  useEffect(() => {
    if (!initRef.current) {
      handleCommandSubmit("startup");
      initRef.current = true; // Set the ref to true after the first run
    }
  }, [handleCommandSubmit]); // Include handleCommandSubmit in the dependency array

  return (
    <div className="App">
      <GameOutput messages={messages} />
      <GameInput onCommandSubmit={handleCommandSubmit} />
      <LocationImage imageSrc={currentLocationImage} />
      {currentGameRef.current && (
        <GameStatus
          inventory={inventory}
          score={score}
          location={location}
          date={date}
          time={time}
          currentLocationDescription={currentLocationDescription}
        />
      )}
    </div>
  );
}

export default App;
