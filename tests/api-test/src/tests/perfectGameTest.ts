import { ApiClient } from '../client/apiClient.js';
import { LocationGraph, ITEM_LOSS_LOCATIONS } from '../utils/locationGraph.js';
import { navigateTo, hasItem, hasEvent, ensureDaytime } from '../utils/gameState.js';
import { ProcessCommandResponse } from '../types/gameTypes.js';

/**
 * Navigate to a location for perfect game - avoids SlipperyRoom but allows death traps
 * Used when we have the right items to survive death traps
 */
async function navigatePerfectGame(
  client: ApiClient,
  graph: LocationGraph,
  destination: string
): Promise<{ success: boolean; error?: string }> {
  const current = client.getCurrentLocation();
  if (!current) {
    return { success: false, error: 'Unknown current location' };
  }

  if (current === destination) {
    return { success: true };
  }

  // Avoid only SlipperyRoom (which steals items), but allow death traps
  const path = graph.findPathWithAvoid(current, destination, ITEM_LOSS_LOCATIONS);
  if (!path) {
    return { success: false, error: `No path from ${current} to ${destination}` };
  }

  for (const direction of path) {
    await client.sendCommand(direction);
    if (client.isGameOver()) {
      return { success: false, error: 'GameOver during navigation' };
    }
  }

  const arrived = client.getCurrentLocation() === destination;
  return { success: arrived };
}

/**
 * Perfect Game Test - Achieves 3000 points (maximum score) and rescue
 *
 * TREASURES (3000 points total):
 * - pearl (150) - LagoonDepths
 * - coins (50) - MaidsQuarters
 * - stocksAndBonds (100) - Office
 * - pricelessPainting (400) - Attic
 * - rubyRing (150) - BoneRoom
 * - emeraldNecklace (150) - NativeVillage
 * - diamonds (300) - GoblinValley (examine GoblinTower to reveal)
 * - deadmansTreasure (200) - DeadmansGulch
 * - aladdinsLamp (300) - ArabianRoom
 * - fissureTreasure (400) - FissureRoom
 * - TreasureChest (400) - TwinPalms (dig)
 * - bundleOfBills (100) - Safe in SittingRoom
 * - potOfGold (300) - RescueBeach (dig)
 */

interface TestStep {
  description: string;
  action: () => Promise<boolean>;
  required: boolean;
}

export async function runPerfectGameTest(client: ApiClient, graph: LocationGraph): Promise<void> {
  console.log('\n🏆 PERFECT GAME TEST - Target: 3000 points + Rescue\n');
  console.log('=' .repeat(60) + '\n');

  // Start a completely fresh game
  await client.sendCommand('new');

  if (client.getCurrentLocation() !== 'Hilltop') {
    console.log(`  Warning: Expected Hilltop, got ${client.getCurrentLocation()}`);
    await client.sendCommand('startup');
  }

  let totalSteps = 0;
  let passedSteps = 0;
  let failedSteps: string[] = [];

  const runStep = async (step: TestStep): Promise<boolean> => {
    totalSteps++;
    process.stdout.write(`  ${step.description}... `);

    try {
      const success = await step.action();
      if (success) {
        passedSteps++;
        console.log('✓');
        return true;
      } else {
        if (step.required) {
          failedSteps.push(step.description);
        }
        console.log(step.required ? '✗ FAILED' : '⚠ skipped');
        return false;
      }
    } catch (error) {
      if (step.required) {
        failedSteps.push(`${step.description}: ${error}`);
      }
      console.log(`✗ ERROR: ${error}`);
      return false;
    }
  };

  // ============================================================
  // PHASE 1: TOWN PREPARATION
  // ============================================================
  console.log('--- PHASE 1: Town Preparation ---\n');

  // Get wallet and money
  await runStep({
    description: 'Navigate to AlleyEnd for wallet',
    action: async () => {
      const nav = await navigateTo(client, graph, 'AlleyEnd', false);
      return nav.success;
    },
    required: true
  });

  await runStep({
    description: 'Examine bushes to find wallet',
    action: async () => {
      await client.sendCommand('examine bushes');
      await client.sendCommand('examine bookshelf');
      return true;
    },
    required: true
  });

  await runStep({
    description: 'Take the wallet',
    action: async () => {
      await client.sendCommand('take wallet');
      return hasItem(client, 'wallet');
    },
    required: true
  });

  await runStep({
    description: 'Return wallet to Constable for 10 gold reward',
    action: async () => {
      const nav = await navigateTo(client, graph, 'ConstablesOffice', false);
      if (!nav.success) return false;
      await client.sendCommand('drop wallet');
      return client.getMoney() >= 10;
    },
    required: true
  });

  console.log(`    Current money: ${client.getMoney()} gold\n`);

  // Buy donuts (essential for boar)
  await runStep({
    description: 'Buy donuts at Bakery (1 gold)',
    action: async () => {
      const nav = await navigateTo(client, graph, 'Bakery', false);
      if (!nav.success) return false;
      await client.sendCommand('buy donuts');
      return hasItem(client, 'donuts');
    },
    required: true
  });

  // Buy boat ticket
  await runStep({
    description: 'Buy boat ticket (5 gold)',
    action: async () => {
      const nav = await navigateTo(client, graph, 'TicketBooth', false);
      if (!nav.success) return false;
      await client.sendCommand('buy ticket');
      return hasItem(client, 'ticket');
    },
    required: true
  });

  console.log(`    Money remaining: ${client.getMoney()} gold\n`);

  // ============================================================
  // PHASE 2: JOURNEY TO ISLAND
  // ============================================================
  console.log('--- PHASE 2: Journey to Island ---\n');

  await runStep({
    description: 'Board boat and trigger shipwreck',
    action: async () => {
      // Board the boat (north from ticket booth)
      await client.sendCommand('north');

      // Go down to cabin
      await client.sendCommand('down');

      if (client.getCurrentLocation() !== 'BoatCabin') {
        console.log(`    Not in cabin: ${client.getCurrentLocation()}`);
        return false;
      }

      // Sleep to trigger shipwreck
      for (let i = 0; i < 5; i++) {
        await client.sendCommand('sleep');
        const loc = client.getCurrentLocation();
        if (loc === 'ShipWreckBeach' || loc === 'ShipWreck') {
          return true;
        }
      }

      // Try rest as alternative
      await client.sendCommand('rest');
      const loc = client.getCurrentLocation();
      return loc === 'ShipWreckBeach' || loc === 'ShipWreck';
    },
    required: true
  });

  console.log(`    Now at: ${client.getCurrentLocation()}\n`);

  // ============================================================
  // PHASE 3: COLLECT ESSENTIAL ITEMS
  // ============================================================
  console.log('--- PHASE 3: Collect Essential Items ---\n');

  // Get matches from shipwreck - need to go east to ShipWreck location
  await runStep({
    description: 'Go to ShipWreck and get matches from BrokenSeaChest',
    action: async () => {
      // Navigate to ShipWreck (east from ShipWreckBeach)
      await client.sendCommand('east');
      if (client.getCurrentLocation() !== 'ShipWreck') {
        console.log(`    Not at ShipWreck: ${client.getCurrentLocation()}`);
        return false;
      }
      await client.sendCommand('examine BrokenSeaChest');
      await client.sendCommand('take matches');
      return hasItem(client, 'matches');
    },
    required: true
  });

  // Get lumber for signal fire
  await runStep({
    description: 'Take lumber from shipwreck',
    action: async () => {
      await client.sendCommand('take lumber');
      return hasItem(client, 'lumber');
    },
    required: false
  });

  // Navigate to mansion area for key items
  await runStep({
    description: 'Navigate to TrophyRoom for dead black cat',
    action: async () => {
      const nav = await navigateTo(client, graph, 'TrophyRoom', false);
      if (!nav.success) return false;
      await client.sendCommand('take deadBlackCat');
      return hasItem(client, 'deadBlackCat');
    },
    required: true
  });

  await runStep({
    description: 'Get wooden stake from ToolShed',
    action: async () => {
      const nav = await navigateTo(client, graph, 'ToolShed', false);
      if (!nav.success) return false;
      await client.sendCommand('take woodenStake');
      return hasItem(client, 'woodenStake');
    },
    required: true
  });

  await runStep({
    description: 'Get hammer from ToolShed',
    action: async () => {
      await client.sendCommand('take hammer');
      return hasItem(client, 'hammer');
    },
    required: true
  });

  await runStep({
    description: 'Get shovel from ToolShed',
    action: async () => {
      await client.sendCommand('take shovel');
      return hasItem(client, 'shovel');
    },
    required: true
  });

  await runStep({
    description: 'Get canteen from EastCellar',
    action: async () => {
      const nav = await navigateTo(client, graph, 'EastCellar', false);
      if (!nav.success) return false;
      await client.sendCommand('take canteen');
      return hasItem(client, 'canteen');
    },
    required: true
  });

  await runStep({
    description: 'Fill canteen with water at Creek',
    action: async () => {
      const nav = await navigateTo(client, graph, 'Creek', false);
      if (!nav.success) return false;
      await client.sendCommand('fill canteen');
      return hasEvent(client, 'canteen_filled');
    },
    required: true
  });

  console.log(`    Inventory: ${client.getInventory().join(', ')}\n`);

  // ============================================================
  // PHASE 4: MANSION TREASURES
  // ============================================================
  console.log('--- PHASE 4: Mansion Treasures ---\n');

  await runStep({
    description: 'Get coins from MaidsQuarters (50 pts)',
    action: async () => {
      const nav = await navigateTo(client, graph, 'MaidsQuarters', false);
      if (!nav.success) return false;
      await client.sendCommand('take coins');
      return hasItem(client, 'coins');
    },
    required: true
  });

  await runStep({
    description: 'Get stocksAndBonds from Office (100 pts)',
    action: async () => {
      const nav = await navigateTo(client, graph, 'Office', false);
      if (!nav.success) return false;
      await client.sendCommand('take stocksAndBonds');
      return hasItem(client, 'stocksAndBonds');
    },
    required: true
  });

  await runStep({
    description: 'Get pricelessPainting from Attic (400 pts)',
    action: async () => {
      const nav = await navigateTo(client, graph, 'Attic', false);
      if (!nav.success) return false;
      await client.sendCommand('take pricelessPainting');
      return hasItem(client, 'pricelessPainting');
    },
    required: true
  });

  // Get safe combination from parrot
  await runStep({
    description: 'Learn safe combination from parrot in SmallRoom',
    action: async () => {
      // SmallRoom is in the caves - need to navigate there
      // First light matches, then navigate to SmallRoom (it's dark)
      await client.sendCommand('light matches');
      const nav = await navigatePerfectGame(client, graph, 'SmallRoom');
      if (!nav.success) {
        console.log(`    Could not navigate to SmallRoom from ${client.getCurrentLocation()}`);
        return false;
      }
      // Light again in case the light went out
      await client.sendCommand('light matches');
      // Look around to trigger parrot action
      await client.sendCommand('look');
      return hasEvent(client, 'learned_combination');
    },
    required: true
  });

  await runStep({
    description: 'Open safe in SittingRoom for bundleOfBills (100 pts)',
    action: async () => {
      // Navigate back to mansion from caves
      const nav = await navigatePerfectGame(client, graph, 'SittingRoom');
      if (!nav.success) {
        console.log(`    Could not navigate to SittingRoom from ${client.getCurrentLocation()}`);
        return false;
      }
      await client.sendCommand('open safe');
      await client.sendCommand('take bundleOfBills');
      return hasItem(client, 'bundleOfBills');
    },
    required: true
  });

  // ============================================================
  // PHASE 5: CAVE TREASURES
  // ============================================================
  console.log('--- PHASE 5: Cave Treasures ---\n');

  await runStep({
    description: 'Get rubyRing from BoneRoom (150 pts)',
    action: async () => {
      const nav = await navigatePerfectGame(client, graph, 'BoneRoom');
      if (!nav.success) return false;
      await client.sendCommand('take rubyRing');
      return hasItem(client, 'rubyRing');
    },
    required: true
  });

  await runStep({
    description: 'Get diamonds from GoblinValley (300 pts)',
    action: async () => {
      const nav = await navigatePerfectGame(client, graph, 'GoblinValley');
      if (!nav.success) {
        console.log(`    Could not navigate to GoblinValley`);
        return false;
      }
      await client.sendCommand('light matches');
      await client.sendCommand('examine GoblinTower');
      await client.sendCommand('take diamonds');
      return hasItem(client, 'diamonds');
    },
    required: true
  });

  await runStep({
    description: 'Get deadmansTreasure from DeadmansGulch (200 pts)',
    action: async () => {
      const nav = await navigatePerfectGame(client, graph, 'DeadmansGulch');
      if (!nav.success) {
        console.log(`    Could not navigate to DeadmansGulch`);
        return false;
      }
      await client.sendCommand('light matches');
      await client.sendCommand('take deadmansTreasure');
      return hasItem(client, 'deadmansTreasure');
    },
    required: true
  });

  await runStep({
    description: 'Get aladdinsLamp from ArabianRoom (300 pts)',
    action: async () => {
      const nav = await navigatePerfectGame(client, graph, 'ArabianRoom');
      if (!nav.success) {
        console.log(`    Could not navigate to ArabianRoom`);
        return false;
      }
      await client.sendCommand('light matches');
      await client.sendCommand('take aladdinsLamp');
      return hasItem(client, 'aladdinsLamp');
    },
    required: true
  });

  await runStep({
    description: 'Get fissureTreasure from FissureRoom (400 pts)',
    action: async () => {
      const nav = await navigatePerfectGame(client, graph, 'FissureRoom');
      if (!nav.success) {
        console.log(`    Could not navigate to FissureRoom`);
        return false;
      }
      await client.sendCommand('light matches');
      await client.sendCommand('take fissureTreasure');
      return hasItem(client, 'fissureTreasure');
    },
    required: true
  });

  // ============================================================
  // PHASE 6: DANGEROUS AREAS (Order matters for path clearing!)
  // ============================================================
  console.log('--- PHASE 6: Dangerous Areas ---\n');

  // Get past boar with donuts FIRST - this clears the path to Cavern/RescueBeach
  await runStep({
    description: 'Navigate through BoarDen (using donuts)',
    action: async () => {
      // The boar action automatically uses donuts when entering
      if (!hasItem(client, 'donuts')) {
        console.log(`    Don't have donuts!`);
        return false;
      }
      const nav = await navigatePerfectGame(client, graph, 'EastBoarDen');
      if (client.isGameOver()) {
        console.log(`    Died at BoarDen!`);
        return false;
      }
      return nav.success || hasEvent(client, 'boar_fed');
    },
    required: true
  });

  // Kill Dracula during day
  await runStep({
    description: 'Ensure daytime for Dracula encounter',
    action: async () => {
      await ensureDaytime(client);
      const hour = client.getGameTime().getHours();
      return hour >= 6 && hour < 20;
    },
    required: true
  });

  await runStep({
    description: 'Kill Dracula in CoffinRoom',
    action: async () => {
      // CoffinRoom is a conditional death trap - need to use unsafe navigation
      // We have stake and hammer, and it's daytime, so we're safe
      const nav = await navigatePerfectGame(client, graph, 'CoffinRoom');
      if (!nav.success) {
        console.log(`    Could not navigate to CoffinRoom from ${client.getCurrentLocation()}`);
        return false;
      }
      if (client.isGameOver()) {
        console.log(`    Died at CoffinRoom!`);
        return false;
      }
      await client.sendCommand('kill dracula');
      return hasEvent(client, 'killed_dracula');
    },
    required: true
  });

  // Get emerald necklace from natives (protected by dead cat)
  await runStep({
    description: 'Get emeraldNecklace from NativeVillage (150 pts)',
    action: async () => {
      // Dead black cat protects from natives - need unsafe navigation
      if (!hasItem(client, 'deadBlackCat') && !hasItem(client, 'deadblackcat')) {
        console.log(`    Don't have dead black cat!`);
        return false;
      }
      const nav = await navigatePerfectGame(client, graph, 'NativeVillage');
      if (!nav.success) {
        console.log(`    Could not navigate to NativeVillage from ${client.getCurrentLocation()}`);
        return false;
      }
      if (client.isGameOver()) {
        console.log(`    Died at NativeVillage!`);
        return false;
      }
      await client.sendCommand('take emeraldNecklace');
      return hasItem(client, 'emeraldNecklace');
    },
    required: true
  });

  // Navigate through salt room (protected by water) - optional, opens path through caves
  await runStep({
    description: 'Navigate through SaltRoom (using water)',
    action: async () => {
      // Verify we have filled canteen
      if (!hasEvent(client, 'canteen_filled')) {
        console.log(`    Canteen not filled!`);
        return false;
      }
      const nav = await navigatePerfectGame(client, graph, 'SaltRoom');
      if (client.isGameOver()) {
        console.log(`    Died in SaltRoom!`);
        return false;
      }
      return nav.success;
    },
    required: false
  });

  // ============================================================
  // PHASE 7: SPECIAL TREASURES
  // ============================================================
  console.log('--- PHASE 7: Special Treasures ---\n');

  // Dig for treasure chest at TwinPalms
  await runStep({
    description: 'Dig for TreasureChest at TwinPalms (400 pts)',
    action: async () => {
      // TwinPalms is near DenseJungle (death trap), use unsafe navigation
      const nav = await navigatePerfectGame(client, graph, 'TwinPalms');
      if (!nav.success) {
        console.log(`    Could not navigate to TwinPalms from ${client.getCurrentLocation()}`);
        return false;
      }
      await client.sendCommand('dig');
      await client.sendCommand('take TreasureChest');
      return hasItem(client, 'TreasureChest');
    },
    required: true
  });

  // Get pearl from lagoon
  await runStep({
    description: 'Get pearl from LagoonDepths (150 pts)',
    action: async () => {
      // Need to swim and dive in lagoon - use unsafe nav as area is near death traps
      const nav = await navigatePerfectGame(client, graph, 'Lagoon');
      if (!nav.success) {
        console.log(`    Could not navigate to Lagoon from ${client.getCurrentLocation()}`);
        return false;
      }
      await client.sendCommand('swim');
      // Navigate to LagoonSwimming first
      await client.sendCommand('north');
      // Then to LagoonDepths
      await client.sendCommand('down');
      await client.sendCommand('take pearl');
      return hasItem(client, 'pearl');
    },
    required: true
  });

  // ============================================================
  // PHASE 8: VICTORY - RESCUE BEACH
  // ============================================================
  console.log('--- PHASE 8: Victory at Rescue Beach ---\n');

  await runStep({
    description: 'Navigate to RescueBeach',
    action: async () => {
      // RescueBeach is connected through areas with death traps, use unsafe navigation
      // Path: through Cavern -> NorthSouthPath -> RescueBeach
      const nav = await navigatePerfectGame(client, graph, 'RescueBeach');
      if (!nav.success) {
        console.log(`    Could not navigate to RescueBeach from ${client.getCurrentLocation()}`);
        return false;
      }
      return nav.success;
    },
    required: true
  });

  // Dig for pot of gold
  await runStep({
    description: 'Dig for potOfGold at RescueBeach (300 pts)',
    action: async () => {
      await client.sendCommand('dig');
      await client.sendCommand('take potOfGold');
      return hasItem(client, 'potOfGold');
    },
    required: true
  });

  // Get driftwood for signal fire
  await runStep({
    description: 'Take driftwood for signal fire',
    action: async () => {
      await client.sendCommand('take driftwood');
      return hasItem(client, 'driftwood') || hasItem(client, 'lumber');
    },
    required: true
  });

  console.log('\n--- Dropping Treasures for Points ---\n');

  // List of all treasures to drop
  const treasures = [
    { name: 'pearl', points: 150 },
    { name: 'coins', points: 50 },
    { name: 'stocksAndBonds', points: 100 },
    { name: 'pricelessPainting', points: 400 },
    { name: 'rubyRing', points: 150 },
    { name: 'emeraldNecklace', points: 150 },
    { name: 'diamonds', points: 300 },
    { name: 'deadmansTreasure', points: 200 },
    { name: 'aladdinsLamp', points: 300 },
    { name: 'fissureTreasure', points: 400 },
    { name: 'TreasureChest', points: 400 },
    { name: 'bundleOfBills', points: 100 },
    { name: 'potOfGold', points: 300 },
  ];

  let pointsScored = 0;
  for (const treasure of treasures) {
    if (hasItem(client, treasure.name)) {
      await client.sendCommand(`drop ${treasure.name}`);
      pointsScored += treasure.points;
      console.log(`  Dropped ${treasure.name}: +${treasure.points} pts (total: ${pointsScored})`);
    } else {
      console.log(`  Missing ${treasure.name}: ${treasure.points} pts not scored`);
    }
  }

  const finalScore = client.getState()?.score || 0;
  console.log(`\n  Final score from game: ${finalScore} / 3000`);

  // Signal for rescue
  await runStep({
    description: 'Signal for rescue (light fire)',
    action: async () => {
      await client.sendCommand('signal');
      return hasEvent(client, 'rescued');
    },
    required: true
  });

  // ============================================================
  // SUMMARY
  // ============================================================
  console.log('\n' + '=' .repeat(60));
  console.log('PERFECT GAME TEST SUMMARY');
  console.log('=' .repeat(60) + '\n');

  console.log(`Steps passed: ${passedSteps}/${totalSteps}`);
  console.log(`Final score: ${finalScore} / 3000 points`);
  console.log(`Rescued: ${hasEvent(client, 'rescued') ? 'YES' : 'NO'}`);

  if (failedSteps.length > 0) {
    console.log('\nFailed steps:');
    failedSteps.forEach(step => console.log(`  - ${step}`));
  }

  const isPerfect = finalScore >= 3000 && hasEvent(client, 'rescued');
  console.log(`\n${isPerfect ? '🏆 PERFECT GAME ACHIEVED!' : '⚠️  Not a perfect game - some treasures missed'}\n`);

  if (failedSteps.length > 0) {
    throw new Error(`Perfect game test had ${failedSteps.length} failed steps`);
  }
}
