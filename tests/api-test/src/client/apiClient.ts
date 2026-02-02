import { SaveGameData, ProcessCommandResponse, WorldData } from '../types/gameTypes.js';
import { TestUser } from '../utils/testUser.js';
import { readFileSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

export class ApiClient {
  private apiEndpoint: string;
  private testUser: TestUser;
  private currentState: SaveGameData | undefined;
  private worldData: WorldData | undefined;

  constructor(apiEndpoint: string, testUser: TestUser) {
    this.apiEndpoint = apiEndpoint;
    this.testUser = testUser;
  }

  async initialize(): Promise<void> {
    // Load world data from file
    const worldDataPath = join(__dirname, '..', '..', '..', '..', 'api', 'Data', 'worldData.json');
    const worldDataContent = readFileSync(worldDataPath, 'utf-8');
    this.worldData = JSON.parse(worldDataContent);

    // Start a new game to get initial state
    await this.sendCommand('startup');
  }

  getWorldData(): WorldData {
    if (!this.worldData) {
      throw new Error('World data not loaded. Call initialize() first.');
    }
    return this.worldData;
  }

  async sendCommand(command: string): Promise<ProcessCommandResponse> {
    const url = `${this.apiEndpoint}/ProcessGameCommand`;

    const maxRetries = 3;
    const timeoutMs = 30000;

    for (let attempt = 0; attempt < maxRetries; attempt++) {
      try {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

        // Base64 encode the client principal (Azure Static Web Apps format)
        const clientPrincipal = {
          userId: this.testUser.userId,
          userRoles: this.testUser.userRoles,
          identityProvider: this.testUser.identityProvider,
          userDetails: this.testUser.userName
        };
        const encodedPrincipal = Buffer.from(JSON.stringify(clientPrincipal)).toString('base64');

        const response = await fetch(url, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'X-MS-CLIENT-PRINCIPAL': encodedPrincipal
          },
          body: JSON.stringify({
            command,
            saveGameData: this.currentState
          }),
          signal: controller.signal
        });

        clearTimeout(timeoutId);

        if (!response.ok) {
          throw new Error(`API error: ${response.status} ${response.statusText}`);
        }

        const result: ProcessCommandResponse = await response.json();

        // Update state
        if (result.saveGameData) {
          this.currentState = result.saveGameData;
        }

        // Small delay between requests
        await this.delay(100);

        return result;
      } catch (error) {
        if (attempt === maxRetries - 1) {
          throw error;
        }
        // Exponential backoff
        await this.delay(Math.pow(2, attempt) * 1000);
      }
    }

    throw new Error('Max retries exceeded');
  }

  async startNewGame(): Promise<ProcessCommandResponse> {
    this.currentState = {
      player: this.testUser.userId,
      aiEmbelleshedDescriptions: false
    };
    return this.sendCommand('startup');
  }

  async saveCheckpoint(slot: number): Promise<ProcessCommandResponse> {
    return this.sendCommand(`save ${slot}`);
  }

  async loadCheckpoint(slot: number): Promise<ProcessCommandResponse> {
    return this.sendCommand(`load ${slot}`);
  }

  // State accessors
  getCurrentLocation(): string | undefined {
    return this.currentState?.currentLocation;
  }

  getInventory(): string[] {
    return this.currentState?.inventory || [];
  }

  getEvents(): Array<{ name?: string; description?: string }> {
    return this.currentState?.events || [];
  }

  getMoney(): number {
    return this.currentState?.money || 0;
  }

  getGameTime(): Date {
    return this.currentState?.currentDateTime
      ? new Date(this.currentState.currentDateTime)
      : new Date();
  }

  isGameOver(): boolean {
    return this.getEvents().some(e => e.name === 'GameOver');
  }

  getState(): SaveGameData | undefined {
    return this.currentState;
  }

  setState(state: SaveGameData): void {
    this.currentState = state;
  }

  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}
