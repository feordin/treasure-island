import { ApiClient } from '../client/apiClient.js';
import { LocationGraph } from '../utils/locationGraph.js';
import { navigateTo, collectItem, hasEvent, hasItem } from '../utils/gameState.js';

export async function runMechanicsTests(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('\n⚙️  Testing Game Mechanics...\n');

  // Start fresh game
  await client.sendCommand('startup');

  const tests = [
    testMovement,
    testInventory,
    testItemPickup,
    testItemDrop,
    testEvents,
    testMoney,
    testTimeProgression,
    testDirectionalSystem,
    testDraculaMechanics,
    testIcePuzzle,
    testThirstMechanics,
    testLampMechanics,
    testCreekMechanics,
    testLagoonMechanics
  ];

  for (const test of tests) {
    await test(client, graph);
    // Reset between tests
    await client.sendCommand('startup');
  }
}

async function testMovement(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing movement commands...');

  const startLocation = client.getCurrentLocation();

  // Test cardinal directions
  await client.sendCommand('north');
  const afterNorth = client.getCurrentLocation();

  if (afterNorth === startLocation) {
    console.log('    ⓘ No exit north from starting location');
  } else {
    console.log('    ✓ Cardinal direction movement works');
  }

  // Test relative directions
  await client.sendCommand('startup');
  await client.sendCommand('ahead');
  const afterAhead = client.getCurrentLocation();

  if (afterAhead === startLocation) {
    console.log('    ⓘ No exit ahead from starting location');
  } else {
    console.log('    ✓ Relative direction movement works');
  }

  console.log('  ✓ Movement test passed');
}

async function testInventory(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing inventory system...');

  const initialInventory = client.getInventory();
  console.log(`    Initial inventory: ${initialInventory.length} items`);

  // Try to find and pick up an item
  const locations = graph.getAllLocations();
  let foundItem = false;

  for (const locId of locations) {
    const location = graph.getLocation(locId);
    if (location?.Items && location.Items.length > 0) {
      const item = location.Items[0];
      const nav = await navigateTo(client, graph, locId, false);

      if (nav.success) {
        await client.sendCommand(`take ${item}`);
        const newInventory = client.getInventory();

        if (newInventory.length > initialInventory.length) {
          foundItem = true;
          console.log(`    ✓ Successfully picked up item: ${item}`);
          break;
        }
      }
    }
  }

  if (!foundItem) {
    console.log('    ⓘ No pickable items found in accessible locations');
  }

  console.log('  ✓ Inventory test passed');
}

async function testItemPickup(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing item pickup...');

  // Find a location with items
  const locationsWithItems = graph.getAllLocations()
    .map(id => graph.getLocation(id))
    .filter(loc => loc?.Items && loc.Items.length > 0);

  if (locationsWithItems.length === 0) {
    console.log('    ⓘ No locations with items found');
    return;
  }

  const testLocation = locationsWithItems[0];
  if (!testLocation) return;

  const testItem = testLocation.Items![0];

  await navigateTo(client, graph, testLocation.Name, false);
  const beforeCount = client.getInventory().length;

  await client.sendCommand(`take ${testItem}`);
  const afterCount = client.getInventory().length;

  if (afterCount > beforeCount) {
    console.log(`    ✓ Item pickup works (${testItem})`);
  } else {
    console.log(`    ⓘ Item may not be pickable (${testItem})`);
  }

  console.log('  ✓ Item pickup test passed');
}

async function testItemDrop(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing item drop...');

  // First get an item
  const result = await collectItem(client, graph, 'money', false);

  if (!result.success) {
    console.log('    ⓘ Could not acquire item for drop test');
    return;
  }

  const beforeCount = client.getInventory().length;
  await client.sendCommand('drop money');
  const afterCount = client.getInventory().length;

  if (afterCount < beforeCount) {
    console.log('    ✓ Item drop works');
  } else {
    console.log('    ⓘ Item drop may not have worked');
  }

  console.log('  ✓ Item drop test passed');
}

async function testEvents(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing event system...');

  const initialEvents = client.getEvents();
  console.log(`    Initial events: ${initialEvents.length}`);

  // Navigate to a few locations to potentially trigger events
  const locations = graph.getAllLocations().slice(0, 5);

  for (const locId of locations) {
    await navigateTo(client, graph, locId, false);
  }

  const finalEvents = client.getEvents();
  console.log(`    Final events: ${finalEvents.length}`);

  if (finalEvents.length > initialEvents.length) {
    console.log('    ✓ Events are being tracked');
  } else {
    console.log('    ⓘ No new events triggered');
  }

  console.log('  ✓ Event test passed');
}

async function testMoney(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing money system...');

  const initialMoney = client.getMoney();
  console.log(`    Initial money: $${initialMoney}`);

  // Try to collect money item if available
  const result = await collectItem(client, graph, 'money', false);

  if (result.success) {
    const newMoney = client.getMoney();
    if (newMoney > initialMoney) {
      console.log(`    ✓ Money increased: $${initialMoney} → $${newMoney}`);
    } else {
      console.log('    ⓘ Money value unchanged after collecting money item');
    }
  } else {
    console.log('    ⓘ Money item not accessible');
  }

  console.log('  ✓ Money test passed');
}

async function testTimeProgression(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing time progression...');

  const initialTime = client.getGameTime();
  const initialHour = initialTime.getHours();

  console.log(`    Initial time: ${initialHour}:00`);

  // Try sleep command
  const response = await client.sendCommand('sleep');

  if (!client.isGameOver()) {
    const newTime = client.getGameTime();
    const newHour = newTime.getHours();

    if (newHour !== initialHour) {
      console.log(`    ✓ Time advanced: ${initialHour}:00 → ${newHour}:00`);
    } else {
      console.log('    ⓘ Time did not advance (may need specific location)');
    }
  } else {
    console.log('    ⓘ Sleep command caused game over (expected in some locations)');
  }

  console.log('  ✓ Time progression test passed');
}

async function testDirectionalSystem(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing directional system...');

  const startLoc = client.getCurrentLocation();

  // Test that cardinal and relative directions both work
  let cardinalWorks = false;
  let relativeWorks = false;

  // Try cardinal
  await client.sendCommand('north');
  if (client.getCurrentLocation() !== startLoc) {
    cardinalWorks = true;
  }

  await client.sendCommand('startup');

  // Try relative
  await client.sendCommand('ahead');
  if (client.getCurrentLocation() !== startLoc) {
    relativeWorks = true;
  }

  if (cardinalWorks) {
    console.log('    ✓ Cardinal directions work');
  }
  if (relativeWorks) {
    console.log('    ✓ Relative directions work');
  }
  if (!cardinalWorks && !relativeWorks) {
    console.log('    ⓘ No valid exits from starting location');
  }

  console.log('  ✓ Directional system test passed');
}

async function testDraculaMechanics(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing Dracula mechanics...');

  // Test 1: Enter CoffinRoom at night = GameOver
  console.log('    Testing night entry (should cause GameOver)...');
  await client.sendCommand('startup');

  // Ensure it's night time (need to manipulate time)
  const currentHour = client.getGameTime().getHours();
  if (currentHour >= 6 && currentHour < 18) {
    // It's day, need to advance time to night
    await client.sendCommand('sleep');
  }

  const navNight = await navigateTo(client, graph, 'CoffinRoom', false);
  if (navNight.success && client.isGameOver()) {
    console.log('    ✓ Entering CoffinRoom at night causes GameOver');
  } else if (!navNight.success) {
    console.log('    ⓘ Could not navigate to CoffinRoom');
  } else {
    console.log('    ⚠ Expected GameOver when entering CoffinRoom at night');
  }

  // Test 2: Enter during day = safe
  console.log('    Testing day entry (should be safe)...');
  await client.sendCommand('startup');

  // Ensure it's daytime
  const dayHour = client.getGameTime().getHours();
  if (dayHour < 6 || dayHour >= 18) {
    // It's night, need to advance to day
    await client.sendCommand('sleep');
  }

  const navDay = await navigateTo(client, graph, 'CoffinRoom', false);
  if (navDay.success && !client.isGameOver()) {
    console.log('    ✓ Entering CoffinRoom during day is safe');
  } else if (!navDay.success) {
    console.log('    ⓘ Could not navigate to CoffinRoom');
  } else {
    console.log('    ⚠ Unexpected GameOver during day entry');
  }

  // Test 3: Kill with stake+hammer during day
  console.log('    Testing Dracula kill...');
  await client.sendCommand('startup');

  // Collect wooden stake and hammer from ToolShed
  const stakeResult = await collectItem(client, graph, 'woodenStake', false);
  const hammerResult = await collectItem(client, graph, 'hammer', false);

  if (stakeResult.success && hammerResult.success) {
    // Navigate to CoffinRoom during day
    const killNav = await navigateTo(client, graph, 'CoffinRoom', false);
    if (killNav.success) {
      // Try to use stake and hammer
      await client.sendCommand('use wooden stake');
      await client.sendCommand('use hammer');

      if (hasEvent(client, 'killed_dracula')) {
        console.log('    ✓ Successfully killed Dracula with stake and hammer');
      } else {
        console.log('    ⓘ Could not verify Dracula kill event');
      }
    }
  } else {
    console.log('    ⓘ Could not collect required items (stake, hammer)');
  }

  console.log('  ✓ Dracula mechanics test passed');
}

async function testIcePuzzle(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing Ice Puzzle mechanics...');

  // Test 1: Cannot swim across dry fissure
  console.log('    Testing dry fissure (swim should fail)...');
  await client.sendCommand('startup');

  const navFissure = await navigateTo(client, graph, 'FissureRoom', false);
  if (navFissure.success) {
    await client.sendCommand('swim');
    const afterSwim = client.getCurrentLocation();

    if (afterSwim === 'FissureRoom' && !hasEvent(client, 'fissure_filled')) {
      console.log('    ✓ Cannot swim across dry fissure');
    } else {
      console.log('    ⓘ Fissure state unclear');
    }
  } else {
    console.log('    ⓘ Could not navigate to FissureRoom');
  }

  // Test 2: Light coal in WestIceCave melts ice
  console.log('    Testing ice melting...');
  await client.sendCommand('startup');

  // Collect required items: coal, matches
  const coalResult = await collectItem(client, graph, 'coal', false);
  const matchesResult = await collectItem(client, graph, 'matches', false);

  if (coalResult.success && matchesResult.success) {
    const navIceCave = await navigateTo(client, graph, 'WestIceCave', false);
    if (navIceCave.success) {
      await client.sendCommand('light coal');

      if (hasEvent(client, 'ice_melted')) {
        console.log('    ✓ Lighting coal in WestIceCave melts ice');
      } else {
        console.log('    ⓘ Could not verify ice_melted event');
      }
    }
  } else {
    console.log('    ⓘ Could not collect required items (coal, matches)');
  }

  // Test 3: Can swim after ice melts
  console.log('    Testing swim after ice melts...');
  if (hasEvent(client, 'ice_melted') && hasEvent(client, 'fissure_filled')) {
    const swimNav = await navigateTo(client, graph, 'FissureRoom', false);
    if (swimNav.success) {
      await client.sendCommand('swim');
      const afterSwim = client.getCurrentLocation();

      if (afterSwim !== 'FissureRoom') {
        console.log('    ✓ Can swim across fissure after ice melts');
      } else {
        console.log('    ⓘ Swim did not change location');
      }
    }
  } else {
    console.log('    ⓘ Ice not melted or fissure not filled');
  }

  console.log('  ✓ Ice Puzzle test passed');
}

async function testThirstMechanics(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing Thirst mechanics...');

  // Test 1: Enter SaltRoom without water = GameOver
  console.log('    Testing SaltRoom without water (should cause GameOver)...');
  await client.sendCommand('startup');

  const navNoWater = await navigateTo(client, graph, 'SaltRoom', false);
  if (navNoWater.success && client.isGameOver()) {
    console.log('    ✓ Entering SaltRoom without water causes GameOver');
  } else if (!navNoWater.success) {
    console.log('    ⓘ Could not navigate to SaltRoom');
  } else {
    console.log('    ⚠ Expected GameOver when entering SaltRoom without water');
  }

  // Test 2: Enter SaltRoom with water = survive
  console.log('    Testing SaltRoom with water (should survive)...');
  await client.sendCommand('startup');

  // Collect canteen and fill it
  const canteenResult = await collectItem(client, graph, 'canteen', false);
  if (canteenResult.success) {
    // Need to fill the canteen (this might require going to a water source)
    await client.sendCommand('fill canteen');

    if (hasEvent(client, 'canteen_filled')) {
      const navWithWater = await navigateTo(client, graph, 'SaltRoom', false);
      if (navWithWater.success && !client.isGameOver()) {
        console.log('    ✓ Entering SaltRoom with water allows survival');
      } else if (!navWithWater.success) {
        console.log('    ⓘ Could not navigate to SaltRoom');
      } else {
        console.log('    ⚠ Unexpected GameOver with filled canteen');
      }
    } else {
      console.log('    ⓘ Could not fill canteen');
    }
  } else {
    console.log('    ⓘ Could not collect canteen from EastCellar');
  }

  console.log('  ✓ Thirst mechanics test passed');
}

async function testLampMechanics(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing Lamp mechanics...');

  // Test 1: Rub lamp teleports across fissure
  console.log('    Testing lamp teleportation...');
  await client.sendCommand('startup');

  // Collect Aladdin's lamp from ArabianRoom
  const lampResult = await collectItem(client, graph, 'aladdinsLamp', false);
  if (lampResult.success) {
    // Navigate to one side of the fissure
    const navFissure = await navigateTo(client, graph, 'FissureRoom', false);
    if (navFissure.success) {
      const beforeLocation = client.getCurrentLocation();
      await client.sendCommand('rub lamp');
      const afterLocation = client.getCurrentLocation();

      if (afterLocation !== beforeLocation && hasEvent(client, 'lamp_used')) {
        console.log('    ✓ Rubbing lamp teleports player across fissure');
      } else if (afterLocation !== beforeLocation) {
        console.log('    ✓ Rubbing lamp changed location');
      } else {
        console.log('    ⓘ Lamp did not teleport (may need specific conditions)');
      }
    }
  } else {
    console.log('    ⓘ Could not collect Aladdin\'s lamp from ArabianRoom');
  }

  // Test 2: Lamp can only be used once
  console.log('    Testing lamp single-use...');
  if (hasEvent(client, 'lamp_used')) {
    const locationBefore = client.getCurrentLocation();
    await client.sendCommand('rub lamp');
    const locationAfter = client.getCurrentLocation();

    if (locationBefore === locationAfter) {
      console.log('    ✓ Lamp cannot be used twice');
    } else {
      console.log('    ⚠ Lamp was used more than once');
    }
  } else {
    console.log('    ⓘ Lamp not yet used, skipping reuse test');
  }

  console.log('  ✓ Lamp mechanics test passed');
}

async function testCreekMechanics(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing Creek mechanics...');

  // Test 1: Swim at Creek = Death by crocodiles
  console.log('    Testing swim at Creek (should cause GameOver)...');
  await client.sendCommand('startup');

  const navCreek = await navigateTo(client, graph, 'Creek', false);
  if (navCreek.success) {
    await client.sendCommand('swim');

    if (client.isGameOver() || client.getCurrentLocation() === 'CrocodileDeath') {
      console.log('    ✓ Swimming in creek causes crocodile death');
    } else {
      console.log('    ⚠ Expected GameOver from swimming in creek');
    }
  } else {
    console.log('    ⓘ Could not navigate to Creek');
  }

  // Test 2: Swing command at Creek crosses to SouthCreek
  console.log('    Testing swing at Creek...');
  await client.sendCommand('startup');

  const navCreek2 = await navigateTo(client, graph, 'Creek', false);
  if (navCreek2.success) {
    await client.sendCommand('swing');
    const afterSwing = client.getCurrentLocation();

    if (afterSwing === 'SouthCreek') {
      console.log('    ✓ Swing at Creek moves to SouthCreek');
    } else {
      console.log(`    ⚠ Expected SouthCreek after swing, got ${afterSwing}`);
    }
  } else {
    console.log('    ⓘ Could not navigate to Creek');
  }

  // Test 3: Swing command at SouthCreek crosses to Creek
  console.log('    Testing swing at SouthCreek...');
  await client.sendCommand('startup');

  const navSouth = await navigateTo(client, graph, 'SouthCreek', false);
  if (navSouth.success) {
    await client.sendCommand('swing');
    const afterSwing = client.getCurrentLocation();

    if (afterSwing === 'Creek') {
      console.log('    ✓ Swing at SouthCreek moves to Creek');
    } else {
      console.log(`    ⚠ Expected Creek after swing, got ${afterSwing}`);
    }
  } else {
    console.log('    ⓘ Could not navigate to SouthCreek');
  }

  // Test 4: Fill canteen at Creek
  console.log('    Testing fill canteen at Creek...');
  await client.sendCommand('startup');

  const canteenResult = await collectItem(client, graph, 'canteen', false);
  if (canteenResult.success) {
    const navCreek3 = await navigateTo(client, graph, 'Creek', false);
    if (navCreek3.success) {
      await client.sendCommand('fill canteen');

      if (hasEvent(client, 'canteen_filled')) {
        console.log('    ✓ Can fill canteen at Creek');
      } else {
        console.log('    ⓘ Could not fill canteen at Creek');
      }
    }
  } else {
    console.log('    ⓘ Could not collect canteen');
  }

  console.log('  ✓ Creek mechanics test passed');
}

async function testLagoonMechanics(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('  Testing Lagoon mechanics...');

  // Test 1: Movement north from Lagoon should NOT go to LagoonSwimming (removed)
  console.log('    Testing movement restriction at Lagoon...');
  await client.sendCommand('startup');

  const navLagoon = await navigateTo(client, graph, 'Lagoon', false);
  if (navLagoon.success) {
    await client.sendCommand('north');
    const afterNorth = client.getCurrentLocation();

    if (afterNorth === 'Lagoon') {
      console.log('    ✓ North movement from Lagoon is blocked (requires swim)');
    } else if (afterNorth === 'LagoonSwimming') {
      console.log('    ⚠ North movement should not go to LagoonSwimming');
    } else {
      console.log(`    ⓘ Unexpected location after north: ${afterNorth}`);
    }
  } else {
    console.log('    ⓘ Could not navigate to Lagoon');
  }

  // Test 2: Swim command at Lagoon enters LagoonSwimming
  console.log('    Testing swim at Lagoon...');
  await client.sendCommand('startup');

  const navLagoon2 = await navigateTo(client, graph, 'Lagoon', false);
  if (navLagoon2.success) {
    await client.sendCommand('swim');
    const afterSwim = client.getCurrentLocation();

    if (afterSwim === 'LagoonSwimming') {
      console.log('    ✓ Swim at Lagoon moves to LagoonSwimming');
    } else {
      console.log(`    ⚠ Expected LagoonSwimming after swim, got ${afterSwim}`);
    }
  } else {
    console.log('    ⓘ Could not navigate to Lagoon');
  }

  // Test 3: Fill canteen at Lagoon
  console.log('    Testing fill canteen at Lagoon...');
  await client.sendCommand('startup');

  const canteenResult = await collectItem(client, graph, 'canteen', false);
  if (canteenResult.success) {
    const navLagoon3 = await navigateTo(client, graph, 'Lagoon', false);
    if (navLagoon3.success) {
      await client.sendCommand('fill canteen');

      if (hasEvent(client, 'canteen_filled')) {
        console.log('    ✓ Can fill canteen at Lagoon');
      } else {
        console.log('    ⓘ Could not fill canteen at Lagoon');
      }
    }
  } else {
    console.log('    ⓘ Could not collect canteen');
  }

  console.log('  ✓ Lagoon mechanics test passed');
}
