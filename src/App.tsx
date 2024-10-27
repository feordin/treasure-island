// src/App.tsx
import React, { useState, useEffect, useRef, useCallback } from 'react';
import './App.css';
import GameOutput from './components/GameOutput';
import GameInput from './components/GameInput';
import LocationImage from './components/LocationImage';
import GameStatus from './components/GameStatus';
import SavedGamesList from './components/SavedGamesList'; // Import the new component
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
var currentLocationDescription: string | undefined = 'Starting location';
var aiEmbelleshedDescriptions: boolean = false;
var money: number | undefined = 0;
var facing: string | undefined = 'south';


function App() {

  const [messages, setMessages] = useState<Message[]>([
    { sender: 'game', text: 'Welcome to your adventure!' },
  ]);

  const initRef = useRef(false); // Ref to track initialization
  const [currentLocationImage, setCurrentLocationImage] = useState<string>('images/foo.png'); // Initial image
  const [savedGames, setSavedGames] = useState<SaveGameData[]>([]); // State for saved games

  const currentGameRef = useRef<SaveGameData | undefined>({
    player: "Test",
    score: 100,
    currentLocation: "Foo",
    currentDateTime: "1978-01-01",
    inventory: [],
    health: 0,
    history: undefined,
    locationChanges: [],
    aiEmbelleshedDescriptions: false,
    money: 0,
    facing: "south"
  });

  const handleCommandSubmit = useCallback(async (command: string) => {
    // Add user's command to messages
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: 'user', text: command },
    ]);

    const processCommand = async (command: string): Promise<{ response: ProcessCommandResponse;}> => {
      const commandRequest: CommandRequest = {
        command: command,
        saveGameData: currentGameRef.current
      }
      const azureResponse = await invokeAzureFunction(commandRequest);
      console.log('Azure response:', azureResponse);
      const parsedResponse: ProcessCommandResponse = typeof azureResponse === 'string' ? JSON.parse(azureResponse) : azureResponse;
      currentGameRef.current = parsedResponse.saveGameData;
      return {
        response: parsedResponse || 'No response from server'
      };
    };

    // Implement game logic here
    var { response } = await processCommand(command);

    // Add game response to messages
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: 'game', text: response.message || 'No response from server' },
    ]);

    if (response.imageFilename) {

      var newImage = "images/" + response.imageFilename;
      // Update the location image if it has changed
      if (newImage !== currentLocationImage) {
        console.log('Update image:', newImage);
        setCurrentLocationImage(newImage);
      }
    }

    console.log('Current game:', currentGameRef.current);
    // update the game status
    if (currentGameRef.current) {
      if (currentGameRef.current.currentLocation && location !== currentGameRef.current.currentLocation) {
        location = currentGameRef.current.currentLocation;
        currentLocationDescription = response.locationDescription;
      }
      if (currentGameRef.current.currentDateTime) {
        date = new Date(currentGameRef.current.currentDateTime).toLocaleDateString('en-US', { timeZone: 'UTC', });
        time = new Date(currentGameRef.current.currentDateTime).toLocaleTimeString('en-US', { timeZone: 'UTC', });
      }
      if (currentGameRef.current.inventory) {
        inventory = currentGameRef.current.inventory;
      }
      if (currentGameRef.current) {
        score = currentGameRef.current.score;
      }
      if (currentGameRef.current) {
        money = currentGameRef.current.money;
      }
      if (currentGameRef.current) {
        facing = currentGameRef.current.facing;
      }
      if (response.savedGames) {
        setSavedGames(response.savedGames);
      }
      aiEmbelleshedDescriptions = currentGameRef.current.aiEmbelleshedDescriptions;
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
          AiEmbelleshedDescriptions={aiEmbelleshedDescriptions}
          money={money}
          facing={facing}
        />
      )}
      <SavedGamesList savedGames={savedGames} /> {/* Add the SavedGamesList component */}
    </div>
  );
}

export default App;
