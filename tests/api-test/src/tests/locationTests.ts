import { ApiClient } from '../client/apiClient.js';
import { LocationGraph } from '../utils/locationGraph.js';
import { navigateTo } from '../utils/gameState.js';

export async function runLocationTests(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('\n📍 Testing Location Navigation...\n');

  // Start fresh game
  await client.sendCommand('startup');

  // Get all locations
  const allLocations = graph.getAllLocations();
  console.log(`Total locations to visit: ${allLocations.length}`);

  let visitedCount = 0;
  let failedLocations: string[] = [];

  for (const locationId of allLocations) {
    const location = graph.getLocation(locationId);
    if (!location) continue;

    // Try to navigate to this location
    const result = await navigateTo(client, graph, locationId, false);

    if (result.success) {
      visitedCount++;
      console.log(`  ✓ ${location.Name || locationId}`);
    } else {
      failedLocations.push(locationId);
      console.log(`  ✗ ${location.Name || locationId} - ${result.error || 'Failed to reach'}`);
    }

    // Brief pause to avoid overwhelming the API
    await new Promise(resolve => setTimeout(resolve, 100));
  }

  console.log(`\nVisited: ${visitedCount}/${allLocations.length} locations`);

  if (failedLocations.length > 0) {
    console.log('\nFailed locations:');
    failedLocations.forEach(loc => console.log(`  - ${loc}`));
    throw new Error(`Failed to visit ${failedLocations.length} locations`);
  }
}
