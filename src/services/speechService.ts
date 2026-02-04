// src/services/speechService.ts
import * as SpeechSDK from 'microsoft-cognitiveservices-speech-sdk';

interface SpeechToken {
  token: string;
  region: string;
}

let cachedToken: SpeechToken | null = null;
let tokenExpiry: number = 0;

// Get API base URL
const getApiUrl = (): string => {
  return process.env.REACT_APP_API_URL || '/api';
};

// Fetch a new token from our backend
async function getToken(): Promise<SpeechToken> {
  // Return cached token if still valid (tokens last 10 min, we refresh at 9)
  if (cachedToken && Date.now() < tokenExpiry) {
    return cachedToken;
  }

  const response = await fetch(`${getApiUrl()}/GetSpeechToken`);
  if (!response.ok) {
    throw new Error('Failed to get speech token');
  }

  cachedToken = await response.json();
  tokenExpiry = Date.now() + 9 * 60 * 1000; // 9 minutes

  return cachedToken!;
}

// Speech-to-Text: Listen to microphone and return transcribed text
export async function speechToText(): Promise<string> {
  const { token, region } = await getToken();

  return new Promise((resolve, reject) => {
    const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(token, region);
    speechConfig.speechRecognitionLanguage = 'en-US';

    const audioConfig = SpeechSDK.AudioConfig.fromDefaultMicrophoneInput();
    const recognizer = new SpeechSDK.SpeechRecognizer(speechConfig, audioConfig);

    recognizer.recognizeOnceAsync(
      (result) => {
        recognizer.close();

        if (result.reason === SpeechSDK.ResultReason.RecognizedSpeech) {
          resolve(result.text);
        } else if (result.reason === SpeechSDK.ResultReason.NoMatch) {
          reject(new Error('No speech detected. Please try again.'));
        } else if (result.reason === SpeechSDK.ResultReason.Canceled) {
          const cancellation = SpeechSDK.CancellationDetails.fromResult(result);
          reject(new Error(`Speech recognition canceled: ${cancellation.reason}`));
        } else {
          reject(new Error('Speech recognition failed'));
        }
      },
      (error) => {
        recognizer.close();
        reject(error);
      }
    );
  });
}

// Speech rate options
export type SpeechRate = 'x-slow' | 'slow' | 'medium' | 'fast' | 'x-fast' | number;

// Voice presets
export interface VoicePreset {
  name: string;
  voice: string;
  pitch?: string;  // e.g., '+5%', '-10%', 'high', 'low'
  style?: string;  // For voices that support styles
}

export const voicePresets: Record<string, VoicePreset> = {
  'narrator': {
    name: 'Narrator',
    voice: 'en-US-GuyNeural',
  },
  'pirate': {
    name: 'Sea Captain',
    voice: 'en-GB-RyanNeural',
    pitch: '-10%',  // Deeper, gruffer voice
  },
  'british': {
    name: 'British Gentleman',
    voice: 'en-GB-ThomasNeural',
  },
  'storyteller': {
    name: 'Storyteller',
    voice: 'en-US-DavisNeural',
  },
  'female': {
    name: 'Female Narrator',
    voice: 'en-GB-SoniaNeural',
  }
};

// Default settings (can be changed by user)
let currentVoice = 'en-GB-RyanNeural';  // Default to "pirate" voice
let currentPitch = '-10%';
let currentRate: SpeechRate = 'fast';  // Slightly faster than normal for snappier narration

// Reference to current synthesizer for stop functionality
let currentSynthesizer: SpeechSDK.SpeechSynthesizer | null = null;

// Set the speech rate (e.g., 'fast', 'x-fast', or 1.5 for 150% speed)
export function setSpeechRate(rate: SpeechRate): void {
  currentRate = rate;
}

// Set the voice (e.g., 'en-US-GuyNeural', 'en-US-JennyNeural')
export function setSpeechVoice(voice: string, pitch?: string): void {
  currentVoice = voice;
  currentPitch = pitch || '0%';
}

// Set voice by preset name
export function setVoicePreset(presetName: string): void {
  const preset = voicePresets[presetName];
  if (preset) {
    currentVoice = preset.voice;
    currentPitch = preset.pitch || '0%';
  }
}

// Get available presets
export function getVoicePresets(): Record<string, VoicePreset> {
  return voicePresets;
}

// Get current settings
export function getSpeechSettings(): { voice: string; rate: SpeechRate; pitch: string } {
  return { voice: currentVoice, rate: currentRate, pitch: currentPitch };
}

// Stop any currently playing speech
export function stopSpeaking(): void {
  if (currentSynthesizer) {
    currentSynthesizer.close();
    currentSynthesizer = null;
  }
}

// Text-to-Speech: Speak the given text aloud
export async function textToSpeech(text: string, rate?: SpeechRate): Promise<void> {
  const { token, region } = await getToken();
  const useRate = rate ?? currentRate;

  return new Promise((resolve, reject) => {
    // Stop any existing speech first
    stopSpeaking();

    const speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(token, region);
    const audioConfig = SpeechSDK.AudioConfig.fromDefaultSpeakerOutput();
    const synthesizer = new SpeechSDK.SpeechSynthesizer(speechConfig, audioConfig);

    // Store reference for stop functionality
    currentSynthesizer = synthesizer;

    // Build SSML for rate and pitch control
    const rateValue = typeof useRate === 'number' ? `${useRate * 100}%` : useRate;
    const ssml = `
      <speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="en-US">
        <voice name="${currentVoice}">
          <prosody rate="${rateValue}" pitch="${currentPitch}">${escapeXml(text)}</prosody>
        </voice>
      </speak>
    `;

    synthesizer.speakSsmlAsync(
      ssml,
      (result) => {
        currentSynthesizer = null;
        synthesizer.close();

        if (result.reason === SpeechSDK.ResultReason.SynthesizingAudioCompleted) {
          resolve();
        } else if (result.reason === SpeechSDK.ResultReason.Canceled) {
          const cancellation = SpeechSDK.CancellationDetails.fromResult(result);
          // Don't reject if manually stopped
          if (cancellation.reason === SpeechSDK.CancellationReason.Error) {
            reject(new Error(`Speech synthesis canceled: ${cancellation.errorDetails}`));
          } else {
            resolve(); // Treat manual cancellation as success
          }
        } else {
          reject(new Error('Speech synthesis failed'));
        }
      },
      (error) => {
        currentSynthesizer = null;
        synthesizer.close();
        reject(error);
      }
    );
  });
}

// Helper to escape XML special characters
function escapeXml(text: string): string {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}

// Check if speech services are available (browser supports it)
export function isSpeechSupported(): boolean {
  return typeof navigator !== 'undefined' &&
         typeof navigator.mediaDevices !== 'undefined' &&
         typeof navigator.mediaDevices.getUserMedia !== 'undefined';
}
