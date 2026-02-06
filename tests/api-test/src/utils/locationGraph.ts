import { LocationNode, WorldData, Location } from '../types/gameTypes.js';

// Known death trap locations and trap locations with no exits
const DEATH_TRAP_LOCATIONS = new Set([
  'SharkBay', 'LagoonShark', 'OceanHomeward', 'Crashed',
  'Battlefield', 'BubblingCauldron', 'SandyTrail', 'LavaLounge',
  'DenseJungle',    // No exits - "lost in jungle" trap
  'NativeVillage',  // Death without donuts/deadCat
  'EastBoarDen',    // Death without food
  'WestBoarDen',    // Death without food
  'SaltRoom',       // Death without water
]);

// Locations that are death traps only under certain conditions
const CONDITIONAL_DEATH_TRAPS = new Set([
  'CoffinRoom',      // Only at night
  'SaltRoom',        // Only without water
  'NativeVillage',   // Only without dead cat
  'EastBoarDen',     // Only without food
  'WestBoarDen',     // Only without food
  'SlipperyRoom'     // Loses all items, not death
]);

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
    if (DEATH_TRAP_LOCATIONS.has(loc.Name)) return true;

    // Check for death-related actions
    if (loc.Actions) {
      for (const action of loc.Actions) {
        if (action.includes('Death') || action === 'LavaDeath' || action === 'BattlefieldDeath') {
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
    return Array.from(CONDITIONAL_DEATH_TRAPS);
  }

  findLocationsWithItem(itemName: string): string[] {
    return Array.from(this.nodes.entries())
      .filter(([_, node]) => node.items.includes(itemName))
      .map(([name, _]) => name);
  }

  // BFS to find shortest path
  findPath(from: string, to: string, avoidDeathTraps: boolean = true): string[] | null {
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
        if (avoidDeathTraps && destNode.isDeathTrap) continue;
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
