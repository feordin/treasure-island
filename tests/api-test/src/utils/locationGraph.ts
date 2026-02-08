import { LocationNode, WorldData, Location } from '../types/gameTypes.js';

// Known death trap locations and trap locations with no exits
// These are ALWAYS avoided by default pathfinding
const DEATH_TRAP_LOCATIONS = new Set([
  'SharkBay', 'LagoonShark', 'OceanHomeward', 'Crashed',
  'Battlefield', 'BubblingCauldron', 'SandyTrail',
  'DenseJungle',    // No exits - "lost in jungle" trap
  'CrocodileDeath', // Eaten by crocodiles in the creek
  'GameOver',       // Generic death location (no exits)
]);

// Conditional death traps - death depends on having items or time of day
// For perfect game, we need to go through these WITH the right items
const CONDITIONAL_DEATH_LOCATIONS = new Set([
  'NativeVillage',  // Death without deadBlackCat
  'EastBoarDen',    // Death without donuts
  'WestBoarDen',    // Death without donuts
  'SaltRoom',       // Death without water in canteen
  'CoffinRoom',     // Death at night without stake+hammer
]);

// Locations that steal items or have other bad effects (not death, but game-ending for perfect run)
// Always avoided by default pathfinding
export const ITEM_LOSS_LOCATIONS = new Set([
  'SlipperyRoom',   // Steals all inventory items
  // Note: SteamRoom wets matches but is the only path to GoblinValley.
  // The test handles this by doing coal/fissure puzzle BEFORE visiting GoblinValley.
]);

// Locations that are death traps only under certain conditions
// Re-export CONDITIONAL_DEATH_LOCATIONS for use by test code
export { CONDITIONAL_DEATH_LOCATIONS };

export class LocationGraph {
  private nodes: Map<string, LocationNode> = new Map();
  private worldData: WorldData;

  constructor(worldData: WorldData) {
    this.worldData = worldData;
    this.buildGraph();
  }

  private buildGraph(): void {
    for (const loc of this.worldData.Locations) {
      const movements = new Map<string, string>();

      if (loc.AllowedMovements) {
        for (const mov of loc.AllowedMovements) {
          for (const dir of mov.Direction) {
            movements.set(dir.toLowerCase(), mov.Destination);
          }
        }
      }

      const node: LocationNode = {
        name: loc.Name,
        description: loc.Description,
        movements,
        items: loc.Items || [],
        actions: loc.Actions || [],
        isDeathTrap: this.isDeathTrap(loc)
      };

      this.nodes.set(loc.Name, node);
    }
  }

  private isDeathTrap(loc: Location): boolean {
    // Check unconditional death traps
    if (DEATH_TRAP_LOCATIONS.has(loc.Name)) return true;

    // Check item loss locations (game-ending for perfect run)
    if (ITEM_LOSS_LOCATIONS.has(loc.Name)) return true;

    // Check for death-related actions (only truly deadly ones)
    const DEADLY_ACTIONS = new Set(['DynamiteDeath', 'LavaDeath', 'BattlefieldDeath']);
    if (loc.Actions) {
      for (const action of loc.Actions) {
        if (DEADLY_ACTIONS.has(action)) {
          return true;
        }
      }
    }

    return false;
  }

  getNode(name: string): LocationNode | undefined {
    return this.nodes.get(name);
  }

  getNeighbors(name: string): string[] {
    const node = this.nodes.get(name);
    if (!node) return [];
    return Array.from(node.movements.values());
  }

  getDirectionTo(from: string, to: string): string | undefined {
    const node = this.nodes.get(from);
    if (!node) return undefined;

    for (const [dir, dest] of node.movements.entries()) {
      if (dest === to) return dir;
    }
    return undefined;
  }

  getAllLocations(): string[] {
    return Array.from(this.nodes.keys());
  }

  getSafeLocations(): string[] {
    return Array.from(this.nodes.entries())
      .filter(([_, node]) => !node.isDeathTrap)
      .map(([name, _]) => name);
  }

  getDeathTrapLocations(): string[] {
    return Array.from(this.nodes.entries())
      .filter(([_, node]) => node.isDeathTrap)
      .map(([name, _]) => name);
  }

  getConditionalDeathTraps(): string[] {
    return Array.from(CONDITIONAL_DEATH_LOCATIONS);
  }

  findLocationsWithItem(itemName: string): string[] {
    return Array.from(this.nodes.entries())
      .filter(([_, node]) => node.items.includes(itemName))
      .map(([name, _]) => name);
  }

  // BFS to find shortest path
  findPath(from: string, to: string, avoidDeathTraps: boolean = true): string[] | null {
    return this.findPathWithAvoid(from, to, avoidDeathTraps ? undefined : new Set());
  }

  // BFS to find shortest path, with custom set of locations to avoid
  // If avoidLocations is undefined, avoids death traps. If empty set, avoids nothing.
  findPathWithAvoid(from: string, to: string, avoidLocations?: Set<string>): string[] | null {
    if (from === to) return [];

    const visited = new Set<string>();
    const queue: Array<{ location: string; path: string[] }> = [
      { location: from, path: [] }
    ];

    while (queue.length > 0) {
      const { location, path } = queue.shift()!;

      if (visited.has(location)) continue;
      visited.add(location);

      const node = this.nodes.get(location);
      if (!node) continue;

      for (const [direction, destination] of node.movements.entries()) {
        if (destination === to) {
          return [...path, direction];
        }

        const destNode = this.nodes.get(destination);
        if (!destNode) continue;

        // Check if we should avoid this destination
        if (avoidLocations === undefined) {
          // Default: avoid death traps
          if (destNode.isDeathTrap) continue;
        } else {
          // Custom: avoid specified locations
          if (avoidLocations.has(destination)) continue;
        }

        if (visited.has(destination)) continue;

        queue.push({
          location: destination,
          path: [...path, direction]
        });
      }
    }

    return null;
  }

  getWorldData(): WorldData {
    return this.worldData;
  }

  getLocation(name: string): Location | undefined {
    return this.worldData.Locations.find(loc => loc.Name === name);
  }

  getLocationCount(): number {
    return this.nodes.size;
  }

  getConnectedLocations(name: string): string[] {
    return this.getNeighbors(name);
  }
}
