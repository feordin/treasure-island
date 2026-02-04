// src/App.tsx
import React, { useState, useEffect, useRef, useCallback } from 'react';
import './App.css';
import GameOutput from './components/GameOutput';
import GameInput from './components/GameInput';
import LocationImage from './components/LocationImage';
import GameStatus from './components/GameStatus';
import SavedGamesList from './components/SavedGamesList';
import MobileHeader from './components/MobileHeader';
import SlideOutDrawer from './components/SlideOutDrawer';
import { ProcessCommandResponse, CommandRequest, SaveGameData } from './modules/ProcessCommandResponse';
import { invokeAzureFunction } from './services/invokeApi';
import { textToSpeech, isSpeechSupported, setSpeechRate, SpeechRate, stopSpeaking } from './services/speechService';


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

  // Mobile drawer states
  const [isStatusDrawerOpen, setIsStatusDrawerOpen] = useState(false);
  const [isSavedGamesDrawerOpen, setIsSavedGamesDrawerOpen] = useState(false);

  // Text-to-speech state
  const [isTtsEnabled, setIsTtsEnabled] = useState(false);
  const [isSpeaking, setIsSpeaking] = useState(false);
  const [speechRate, setSpeechRateState] = useState<SpeechRate>('fast');

  // Available speech rates to cycle through
  const speechRates: SpeechRate[] = ['slow', 'medium', 'fast', 'x-fast'];
  const speechRateLabels: Record<string, string> = {
    'slow': '🔊 Slow',
    'medium': '🔊 Normal',
    'fast': '🔊 Fast',
    'x-fast': '🔊 Faster'
  };

  const handleTtsToggle = () => {
    if (!isTtsEnabled) {
      // Turning on
      setIsTtsEnabled(true);
    } else {
      // Already on - cycle through speeds
      const currentIndex = speechRates.indexOf(speechRate as any);
      const nextIndex = (currentIndex + 1) % speechRates.length;
      const newRate = speechRates[nextIndex];
      setSpeechRateState(newRate);
      setSpeechRate(newRate);
    }
  };

  const handleTtsOff = () => {
    setIsTtsEnabled(false);
    stopSpeaking();
  };

  const handleStopSpeaking = () => {
    stopSpeaking();
    setIsSpeaking(false);
  };

  const currentGameRef = useRef<SaveGameData | undefined>({
    player: "Test",
    score: 100,
    currentLocation: "Foo",
    currentDateTime: "1978-01-01",
    inventory: [],
    health: 0,
    commandHistory: undefined,
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
    const gameMessage = response.message || 'No response from server';
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: 'game', text: gameMessage },
    ]);

    // Speak the response if TTS is enabled
    if (isTtsEnabled && gameMessage) {
      setIsSpeaking(true);
      try {
        await textToSpeech(gameMessage);
      } catch (error) {
        console.error('Text-to-speech error:', error);
      } finally {
        setIsSpeaking(false);
      }
    }

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
        // Use display name for UI, fall back to internal name if not available
        location = currentGameRef.current.currentLocationDisplayName || currentGameRef.current.currentLocation;
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

  }, [currentLocationImage, isTtsEnabled]);

  useEffect(() => {
    if (!initRef.current) {
      handleCommandSubmit("startup");
      initRef.current = true; // Set the ref to true after the first run
    }
  }, [handleCommandSubmit]); // Include handleCommandSubmit in the dependency array

  return (
    <div className="App">
      {/* Mobile Header - only visible on mobile */}
      <MobileHeader
        onStatusClick={() => setIsStatusDrawerOpen(true)}
        onSavedGamesClick={() => setIsSavedGamesDrawerOpen(true)}
        locationName={location}
        isTtsEnabled={isTtsEnabled}
        onTtsToggle={handleTtsToggle}
        onTtsOff={handleTtsOff}
        onStopSpeaking={handleStopSpeaking}
        isSpeaking={isSpeaking}
        speechRateLabel={speechRateLabels[speechRate] || '🔊'}
      />

      {/* Desktop TTS toggle */}
      {isSpeechSupported() && (
        <div className="desktop-tts-controls">
          {isSpeaking ? (
            <button
              className="desktop-tts-toggle speaking"
              onClick={handleStopSpeaking}
              title="Stop speaking"
            >
              ⏹️
            </button>
          ) : (
            <button
              className={`desktop-tts-toggle ${isTtsEnabled ? 'active' : ''}`}
              onClick={handleTtsToggle}
              title={isTtsEnabled ? `Voice: ${speechRate} (click to change speed)` : 'Enable voice narration'}
            >
              {isTtsEnabled ? '🔊' : '🔇'}
            </button>
          )}
          {isTtsEnabled && (
            <>
              <span className="tts-rate-label">{speechRate}</span>
              <button
                className="desktop-tts-off"
                onClick={handleTtsOff}
                title="Turn off voice"
              >
                ✕
              </button>
            </>
          )}
        </div>
      )}

      {/* Main content */}
      <LocationImage imageSrc={currentLocationImage} />
      <GameOutput messages={messages} />
      <GameInput onCommandSubmit={handleCommandSubmit} />

      {/* Desktop-only components */}
      <div className="desktop-only">
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
        <SavedGamesList savedGames={savedGames} />
      </div>

      {/* Mobile Drawers */}
      <SlideOutDrawer
        isOpen={isStatusDrawerOpen}
        onClose={() => setIsStatusDrawerOpen(false)}
        position="left"
        title="Game Status"
      >
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
      </SlideOutDrawer>

      <SlideOutDrawer
        isOpen={isSavedGamesDrawerOpen}
        onClose={() => setIsSavedGamesDrawerOpen(false)}
        position="right"
        title="Saved Games"
      >
        <SavedGamesList savedGames={savedGames} />
      </SlideOutDrawer>
    </div>
  );
}

export default App;
