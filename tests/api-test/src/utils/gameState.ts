import { ApiClient } from '../client/apiClient.js';
import { LocationGraph } from './locationGraph.js';
import { ProcessCommandResponse } from '../types/gameTypes.js';

export const CHECKPOINT_SLOTS = {
  TOWN_START: 1,
  BEFORE_BOAT: 2,
  ISLAND_START: 3,
  MANSION_ENTRANCE: 4,
  UNDERGROUND_ENTRANCE: 5,
  BEFORE_DEATH_TRAP: 6,
  BEFORE_DRACULA: 7,
  BEFORE_ICE_PUZZLE: 8,
  TEMP: 9
};

export async function navigateTo(
  client: ApiClient,
  graph: LocationGraph,
  destination: string,
  verbose: boolean = false
): Promise<{ success: boolean; response?: ProcessCommandResponse; error?: string }> {
  const current = client.getCurrentLocation();
  if (!current) {
    return { success: false, error: 'Unknown current location' };
  }

  if (current === destination) {
    return { success: true };
  }

  const path = graph.findPath(current, destination, true);  // Avoid death traps
  if (!path) {
    return { success: false, error: `No path from ${current} to ${destination}` };
  }

  if (verbose) {
    console.log(`  Navigating: ${current} -> ${destination} via [${path.join(', ')}]`);
  }

  let response: ProcessCommandResponse | undefined;
  for (const direction of path) {
    response = await client.sendCommand(direction);

    if (client.isGameOver()) {
      return { success: false, response, error: 'GameOver during navigation' };
    }
  }

  const arrived = client.getCurrentLocation() === destination;
  return { success: arrived, response };
}

export async function collectItem(
  client: ApiClient,
  graph: LocationGraph,
  itemName: string,
  verbose: boolean = false
): Promise<{ success: boolean; error?: string }> {
  // Check if already in inventory
  if (client.getInventory().includes(itemName)) {
    return { success: true };
  }

  // Find location with item
  const locations = graph.findLocationsWithItem(itemName);
  if (locations.length === 0) {
    return { success: false, error: `Item ${itemName} not found in any location` };
  }

  // Navigate to first location that has the item
  const nav = await navigateTo(client, graph, locations[0], verbose);
  if (!nav.success) {
    return { success: false, error: nav.error };
  }

  // Take the item
  const response = await client.sendCommand(`take ${itemName}`);
  const hasItem = client.getInventory().some(i =>
    i.toLowerCase() === itemName.toLowerCase()
  );

  return { success: hasItem };
}

export async function collectItems(
  client: ApiClient,
  graph: LocationGraph,
  items: string[],
  verbose: boolean = false
): Promise<{ success: boolean; collected: string[]; failed: string[] }> {
  const collected: string[] = [];
  const failed: string[] = [];

  for (const item of items) {
    const result = await collectItem(client, graph, item, verbose);
    if (result.success) {
      collected.push(item);
    } else {
      failed.push(item);
    }
  }

  return { success: failed.length === 0, collected, failed };
}

export async function setGameTime(
  client: ApiClient,
  targetHour: number,
  verbose: boolean = false
): Promise<void> {
  // Use sleep command to advance time
  // Each sleep advances time by 8 hours
  // IMPORTANT: Use getUTCHours() to match C# backend's DateTime.Hour interpretation
  let currentHour = client.getGameHourUTC();
  let maxAttempts = 10; // Safety limit to prevent infinite loop

  while (currentHour !== targetHour && maxAttempts > 0) {
    if (verbose) {
      console.log(`  Advancing time: ${currentHour} -> ${targetHour}`);
    }

    await client.sendCommand('sleep');
    currentHour = client.getGameHourUTC();
    maxAttempts--;

    // Safety check to prevent infinite loop
    if (client.isGameOver()) break;
  }
}

export async function ensureDaytime(client: ApiClient): Promise<void> {
  // Daytime is 6-19 (6am to 7pm) per C# backend's hour check
  // Use UTC to match backend's DateTime.Hour
  let hour = client.getGameHourUTC();
  let maxAttempts = 5;
  while ((hour < 6 || hour >= 20) && maxAttempts > 0) {
    await client.sendCommand('sleep');
    hour = client.getGameHourUTC();
    maxAttempts--;
    if (client.isGameOver()) break;
  }
}

export async function ensureNighttime(client: ApiClient): Promise<void> {
  // Nighttime is 20-5 (8pm to 5am) per C# backend's hour check
  // Use UTC to match backend's DateTime.Hour
  let hour = client.getGameHourUTC();
  let maxAttempts = 5;
  while (hour >= 6 && hour < 20 && maxAttempts > 0) {
    await client.sendCommand('sleep');
    hour = client.getGameHourUTC();
    maxAttempts--;
    if (client.isGameOver()) break;
  }
}

export function hasEvent(client: ApiClient, eventName: string): boolean {
  return client.getEvents().some(e => e.name === eventName);
}

export function hasItem(client: ApiClient, itemName: string): boolean {
  return client.getInventory().some(i =>
    i.toLowerCase() === itemName.toLowerCase()
  );
}
