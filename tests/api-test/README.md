# Treasure Island API Test Suite

Comprehensive test suite for the Treasure Island game API. Tests all locations, game mechanics, and item interactions via direct API calls.

## Features

- **Location Navigation Tests**: Visits every location in the game and verifies navigation
- **Game Mechanics Tests**: Tests movement, inventory, events, money, time progression
- **Item Tests**: Tests item pickup, drop, collection, and persistence

## Prerequisites

- Node.js 18+
- Azure Functions Core Tools (for running API locally)
- API running at `http://localhost:7071/api` (or configure via `API_ENDPOINT` env var)

## Installation

```bash
npm install
```

## Running Tests

### Run all tests
```bash
npm test
```

### Run specific test category
```bash
npm run test:locations  # Only location navigation tests
npm run test:mechanics  # Only game mechanics tests
npm run test:items      # Only item interaction tests
```

### Configure API endpoint
```bash
API_ENDPOINT=http://localhost:7071/api npm test
```

## Project Structure

```
src/
├── index.ts                 # Main test runner
├── client/
│   └── apiClient.ts        # API client with retry logic
├── types/
│   └── gameTypes.ts        # TypeScript type definitions
├── utils/
│   ├── gameState.ts        # Game state helpers (navigate, collect items, etc.)
│   ├── locationGraph.ts    # Location graph for pathfinding
│   └── testUser.ts         # Test user generation
└── tests/
    ├── locationTests.ts    # Location navigation tests
    ├── mechanicsTests.ts   # Game mechanics tests
    └── itemTests.ts        # Item interaction tests
```

## Test Categories

### 1. Location Tests (`locationTests.ts`)
- Attempts to visit every location in the world
- Reports which locations are reachable
- Identifies unreachable or broken navigation paths

### 2. Mechanics Tests (`mechanicsTests.ts`)
- **Movement**: Cardinal and relative directions
- **Inventory**: Item pickup and inventory tracking
- **Item Pickup**: Collecting items from locations
- **Item Drop**: Dropping items from inventory
- **Events**: Event system tracking
- **Money**: Currency collection and tracking
- **Time Progression**: Sleep/rest mechanics
- **Directional System**: Relative vs cardinal direction translation

### 3. Item Tests (`itemTests.ts`)
- **Item Locations**: Maps all items to their locations
- **Item Collection**: Tests collecting individual items
- **Multiple Items**: Tests collecting sets of items
- **Item Persistence**: Verifies items persist across locations
- **Specific Items**: Tests key game items (money, shovel, ticket, map, key)

## Helper Utilities

### Navigation
```typescript
await navigateTo(client, graph, 'TownSquare', verbose);
```

### Item Collection
```typescript
await collectItem(client, graph, 'shovel', verbose);
await collectItems(client, graph, ['money', 'ticket', 'shovel'], verbose);
```

### Time Management
```typescript
await setGameTime(client, 12);  // Set to noon
await ensureDaytime(client);
await ensureNighttime(client);
```

### State Checks
```typescript
hasEvent(client, 'TicketPurchased');
hasItem(client, 'shovel');
```

## API Client

The `ApiClient` class provides:
- Automatic retry with exponential backoff
- Request timeout handling
- State management (location, inventory, events, money, time)
- User authentication headers for Azure Static Web Apps
- World data loading from local JSON file

## Exit Codes

- `0`: All tests passed
- `1`: One or more tests failed

## Notes

- Tests automatically reset game state between test suites
- The API client includes built-in delays to avoid overwhelming the API
- Failed API calls are retried up to 3 times with exponential backoff
- World data is loaded from `../../api/Data/worldData.json`
