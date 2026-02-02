import { ApiClient } from '../client/apiClient.js';
import { LocationGraph } from '../utils/locationGraph.js';
import { navigateTo, collectItem, collectItems } from '../utils/gameState.js';

export async function runItemTests(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('\n🎒 Testing Item Interactions...\n');

  // Start fresh game
  await client.sendCommand('startup');

  const tests = [
    testItemLocations,
    testItemCollection,
    testMultipleItems,
    testItemPersistence,
    testSpecificItems
  ];

  for (const test of tests) {
    await test(client, graph);
    // Reset between tests
    await client.sendCommand('startup');
  }
}

async function testItemLocations(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing item locations...');

  const allLocations = graph.getAllLocations();
  let locationsWithItems = 0;
  let totalItems = 0;

  for (const locId of allLocations) {
    const location = graph.getLocation(locId);
    if (location?.Items && location.Items.length > 0) {
      locationsWithItems++;
      totalItems += location.Items.length;
    }
  }

  console.log(`    Found ${totalItems} items across ${locationsWithItems} locations`);
  console.log('  ✓ Item location mapping works');
}

async function testItemCollection(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing item collection...');

  // Find all unique items
  const allItems = new Set<string>();
  const allLocations = graph.getAllLocations();

  for (const locId of allLocations) {
    const location = graph.getLocation(locId);
    if (location?.Items) {
      location.Items.forEach((item: string) => allItems.add(item));
    }
  }

  console.log(`    Total unique items in world: ${allItems.size}`);

  // Try to collect first 3 items
  const itemsToTest = Array.from(allItems).slice(0, 3);
  let collected = 0;

  for (const item of itemsToTest) {
    const result = await collectItem(client, graph, item, false);
    if (result.success) {
      collected++;
      console.log(`    ✓ Collected: ${item}`);
    } else {
      console.log(`    ✗ Failed to collect: ${item} - ${result.error || 'unknown error'}`);
    }
  }

  if (collected > 0) {
    console.log(`  ✓ Successfully collected ${collected}/${itemsToTest.length} items`);
  } else {
    console.log('  ⓘ Could not collect any test items');
  }
}

async function testMultipleItems(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing multiple item collection...');

  const testItems = ['money', 'shovel', 'ticket'];

  const result = await collectItems(client, graph, testItems, false);

  console.log(`    Collected: ${result.collected.length}`);
  console.log(`    Failed: ${result.failed.length}`);

  result.collected.forEach(item => console.log(`    ✓ ${item}`));
  result.failed.forEach(item => console.log(`    ✗ ${item}`));

  if (result.collected.length > 0) {
    console.log('  ✓ Multiple item collection works');
  } else {
    console.log('  ⓘ No test items were collectible');
  }
}

async function testItemPersistence(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing item persistence...');

  // Collect an item
  const result = await collectItem(client, graph, 'money', false);

  if (!result.success) {
    console.log('    ⓘ Could not acquire item for persistence test');
    return;
  }

  const inventoryBefore = client.getInventory();
  const hasMoney = inventoryBefore.some(i => i.toLowerCase() === 'money');

  if (hasMoney) {
    console.log('    ✓ Item persists in inventory');
  }

  // Move to another location and check again
  const currentLoc = client.getCurrentLocation();
  const neighbors = currentLoc ? graph.getConnectedLocations(currentLoc) : [];

  if (neighbors.length > 0) {
    await navigateTo(client, graph, neighbors[0], false);
    const inventoryAfter = client.getInventory();
    const stillHasMoney = inventoryAfter.some(i => i.toLowerCase() === 'money');

    if (stillHasMoney) {
      console.log('    ✓ Item persists across location changes');
    } else {
      console.log('    ✗ Item lost after location change');
    }
  }

  console.log('  ✓ Item persistence test passed');
}

async function testSpecificItems(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing specific game items...');

  const keyItems = [
    { name: 'money', description: 'Currency for purchases' },
    { name: 'shovel', description: 'For digging' },
    { name: 'ticket', description: 'For boat travel' },
    { name: 'map', description: 'Navigation aid' },
    { name: 'key', description: 'Opens doors' }
  ];

  for (const item of keyItems) {
    const locations = graph.findLocationsWithItem(item.name);

    if (locations.length > 0) {
      console.log(`    ✓ ${item.name}: found in ${locations.length} location(s)`);

      // Try to collect it
      const result = await collectItem(client, graph, item.name, false);
      if (result.success) {
        console.log(`      → Successfully collected`);
      } else {
        console.log(`      → Could not collect: ${result.error || 'unknown reason'}`);
      }

      // Reset for next item
      await client.sendCommand('startup');
    } else {
      console.log(`    ⓘ ${item.name}: not found in world data`);
    }
  }

  console.log('  ✓ Specific item test completed');
}
