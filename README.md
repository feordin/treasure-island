# Treasure Island

A modern web-based remake of the classic text adventure game originally created by **Richard Erwin** in the 1980s. This version brings the beloved DOS-era game into the modern age with a React frontend, Azure Functions backend, and AI-enhanced features.

## About the Game

Treasure Island is a text-based adventure game set in the 17th century Caribbean. Players explore a mysterious island filled with treasure, danger, and adventure. Navigate through jungle trails, explore an abandoned mansion, delve into underground caverns, and uncover the secrets of the island.

### Features

- Classic text adventure gameplay with natural language input
- AI-powered command parsing for flexible player input
- Save/load game functionality with multiple save slots
- AI-enhanced location descriptions (optional embellishment mode)
- Fortune telling and prayer responses powered by AI
- Pirate-themed text-to-speech narration
- Location images generated with AI
- Responsive design for desktop and mobile

## Architecture

This is an **Azure Static Web App** with an integrated Azure Functions API backend.

```
treasure-island/
├── src/                    # React frontend (TypeScript)
├── api/                    # Azure Functions backend (.NET 8.0)
│   ├── Erwin.Games.TreasureIsland.Commands/
│   ├── Erwin.Games.TreasureIsland.Actions/
│   ├── Erwin.Games.TreasureIsland.Models/
│   ├── Erwin.Games.TreasureIsland.Persistence/
│   └── Data/               # World data JSON files
├── public/                 # Static assets and location images
└── UpsertWorldData/        # Utility to upload world data to Cosmos DB
```

### Tech Stack

- **Frontend**: React 18 with TypeScript, Create React App
- **Backend**: Azure Functions (.NET 8.0, isolated worker model)
- **Database**: Azure Cosmos DB
- **AI Services**: Azure OpenAI (GPT models for command parsing and content generation)
- **Hosting**: Azure Static Web Apps

## Prerequisites

### For Frontend Development
- Node.js 18+ and npm
- A running backend API (local or deployed)

### For Backend Development
- .NET 8.0 SDK
- Azure Functions Core Tools v4
- Azure Cosmos DB account (or emulator)
- Azure OpenAI resource

## Local Development

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/treasure-island.git
cd treasure-island
```

### 2. Configure the Backend

Create `api/local.settings.json` with your Azure credentials:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDBEndpoint": "https://your-cosmos-account.documents.azure.com:443/",
    "CosmosDBKey": "your-cosmos-db-key",
    "AzureAIStudioEndpoint": "https://your-openai-endpoint.openai.azure.com/",
    "AzureAIStudioApiKey": "your-openai-api-key"
  }
}
```

### 3. Upload World Data to Cosmos DB

The game world is defined in `api/Data/worldData.json`. Upload it to your Cosmos DB:

```bash
cd UpsertWorldData
dotnet run ../api/Data/worldData.json
```

Alternatively, world data is automatically uploaded during the API build process.

### 4. Start the Backend API

```bash
cd api
func start
```

The API will run at `http://localhost:7071`.

### 5. Configure the Frontend

Create `.env.local` in the project root:

```
REACT_APP_API_URL=http://localhost:7071/api
```

### 6. Start the Frontend

```bash
npm install
npm start
```

The app will open at `http://localhost:3000`.

### Running Frontend and Backend Together

For the best local development experience, run both in separate terminals:

**Terminal 1 - Backend:**
```bash
cd api
func start
```

**Terminal 2 - Frontend:**
```bash
npm start
```

## Azure Static Web App Deployment

### Automatic Deployment

This project is configured for automatic deployment via GitHub Actions. When you push to the `main` branch:

1. The React app is built and deployed to Azure Static Web Apps
2. The Azure Functions API is deployed alongside the static content
3. World data is automatically uploaded to Cosmos DB during the build

### Manual Deployment

1. Create an Azure Static Web App resource in the Azure Portal
2. Connect it to your GitHub repository
3. Configure the following build settings:
   - **App location**: `/`
   - **API location**: `api`
   - **Output location**: `build`

4. Add the following Application Settings in the Azure Portal:
   - `CosmosDBEndpoint`
   - `CosmosDBKey`
   - `AzureAIStudioEndpoint`
   - `AzureAIStudioApiKey`

### Static Web App Configuration

The `staticwebapp.config.json` file configures routing and authentication:

```json
{
  "routes": [
    {
      "route": "/api/*",
      "allowedRoles": ["authenticated"]
    }
  ],
  "navigationFallback": {
    "rewrite": "/index.html"
  }
}
```

## Game Commands

The game accepts natural language input, but here are some standard commands:

| Command | Description |
|---------|-------------|
| `north`, `south`, `east`, `west` | Move in cardinal directions |
| `ahead`, `behind`, `left`, `right` | Move in relative directions |
| `look` | Examine your surroundings |
| `take [item]` | Pick up an item |
| `drop [item]` | Drop an item from inventory |
| `examine [item]` | Look closely at an item |
| `inventory` | View your items |
| `save [slot]` | Save your game |
| `load [slot]` | Load a saved game |
| `help` | Show available commands |

## Project Structure Details

### Frontend Components

- `App.tsx` - Main game loop and state management
- `GameOutput.tsx` - Displays game messages and responses
- `GameInput.tsx` - Player command input
- `LocationImage.tsx` - Shows AI-generated location images
- `GameStatus.tsx` - Displays player stats (money, score, health)
- `SavedGamesList.tsx` - Manage saved games

### Backend Command Pattern

Commands are processed through:
1. **AI Parser** - Converts natural language to standardized commands
2. **CommandFactory** - Routes to appropriate command handler
3. **Command Execution** - Processes the command and updates game state
4. **ActionFactory** - Triggers location-based events

### Key Files

- `api/Data/worldData.json` - Complete game world definition
- `api/Data/commands.json` - Command aliases and help text
- `public/images/` - Location images

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## Credits

- **Original Game**: Richard Erwin (1980s DOS version)
- **Modern Remake**: Built with Azure, React, and AI technologies
- **Location Images**: Generated with Midjourney AI

## License

This project is a tribute to the original Treasure Island game. Please respect the original creator's work.
