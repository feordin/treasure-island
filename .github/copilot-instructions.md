# Copilot Instructions for Treasure Island

## Project Overview

Treasure Island is a modern web-based remake of a classic 1980s text adventure game. The project is an **Azure Static Web App** with:
- **Frontend**: React 18 with TypeScript (Create React App)
- **Backend**: Azure Functions (.NET 8.0, isolated worker model)
- **Database**: Azure Cosmos DB
- **AI Services**: Azure OpenAI (GPT models for command parsing and content generation)

## Architecture

```
treasure-island/
├── src/                    # React frontend (TypeScript)
│   ├── components/         # React components
│   ├── services/          # API services
│   └── modules/           # Game logic modules
├── api/                    # Azure Functions backend (.NET 8.0)
│   ├── Erwin.Games.TreasureIsland.Commands/    # Command handlers
│   ├── Erwin.Games.TreasureIsland.Actions/     # Location actions
│   ├── Erwin.Games.TreasureIsland.Models/      # Data models
│   ├── Erwin.Games.TreasureIsland.Persistence/ # Database layer
│   └── Data/               # World data JSON files
├── tests/                  # Test projects
│   ├── AgentTests/        # .NET tests
│   └── api-test/          # API integration tests
├── public/                 # Static assets and location images
└── UpsertWorldData/        # Utility to upload world data to Cosmos DB
```

## Build and Test Commands

### Frontend
- **Install dependencies**: `npm install`
- **Start dev server**: `npm start` (runs on http://localhost:3000)
- **Build production**: `npm run build`
- **Run tests**: `npm test`
- **Build with local env**: `npm run build:local`

### Backend (.NET API)
- **Build**: `dotnet build` (from `api/` directory)
- **Run locally**: `func start` (from `api/` directory, runs on http://localhost:7071)
- **Test**: `dotnet test` (from repository root to run all tests)

### Full Stack Development
Run both frontend and backend simultaneously in separate terminals:
- Terminal 1: `cd api && func start`
- Terminal 2: `npm start`

## Code Organization

### Frontend (TypeScript/React)
- **Components**: Located in `src/components/`, each with its own CSS file
- **Services**: API interaction logic in `src/services/`
- **Styling**: Use separate CSS files per component (e.g., `LocationImage.css` for `LocationImage.tsx`)
- **TypeScript**: Strongly typed, use interfaces for props and state

### Backend (.NET)
- **Commands**: Command pattern in `api/Erwin.Games.TreasureIsland.Commands/`
- **Actions**: Location-based events in `api/Erwin.Games.TreasureIsland.Actions/`
- **Models**: Data models in `api/Erwin.Games.TreasureIsland.Models/`
- **Azure Functions**: HTTP triggers in root `api/` directory

## Code Conventions

### General
- Follow existing code style and patterns in each language/framework
- Keep changes minimal and focused
- Test changes before committing

### TypeScript/React
- Use TypeScript for type safety
- Follow React 18 best practices
- Use functional components with hooks
- Keep component files focused and single-purpose

### C#/.NET
- Use .NET 8.0 features
- Follow C# naming conventions (PascalCase for public members, camelCase for private)
- Use nullable reference types (`enable`)
- Leverage implicit usings

### CSS
- Create separate CSS files for each component
- Use responsive design patterns (mobile-first where appropriate)
- Follow existing naming patterns in the codebase

## Testing

### Frontend Tests
- Test framework: Jest with React Testing Library
- Location: `src/` directory (e.g., `App.test.tsx`)
- Run with: `npm test`

### Backend Tests
- Test framework: xUnit
- Location: `tests/AgentTests/`
- Run with: `dotnet test`

### Integration Tests
- Location: `tests/api-test/`
- TypeScript-based API integration tests

## Important Files

### Configuration
- `package.json` - Frontend dependencies and scripts
- `api/api.csproj` - Backend .NET project configuration
- `api/host.json` - Azure Functions configuration
- `staticwebapp.config.json` - Azure Static Web App routing and auth
- `.env` - Environment variables (not committed)
- `api/local.settings.json` - Backend local settings (not committed)

### Game Data
- `api/Data/worldData.json` - Complete game world definition
- `api/Data/commands.json` - Command aliases and help text

### Deployment
- `.github/workflows/azure-static-web-apps-*.yml` - GitHub Actions CI/CD
- `api/upsertWorldDataBuildTarget.targets` - Auto-upload world data during build

## Development Workflow

1. **Local Setup**:
   - Create `api/local.settings.json` with Azure credentials
   - Create `.env.local` with `REACT_APP_API_URL=http://localhost:7071/api`
   - Run `npm install` for frontend dependencies
   - Upload world data: `cd UpsertWorldData && dotnet run ../api/Data/worldData.json`

2. **Making Changes**:
   - Frontend changes: Edit files in `src/`, see changes live with `npm start`
   - Backend changes: Edit files in `api/`, restart with `func start`
   - Test your changes locally before committing

3. **Deployment**:
   - Push to `main` branch triggers automatic GitHub Actions deployment
   - Azure Static Web App builds and deploys both frontend and API
   - World data is automatically uploaded during build

## Common Patterns

### Command Processing Flow
1. User input → AI Parser (converts to standardized command)
2. CommandFactory → Routes to appropriate command handler
3. Command Execution → Updates game state
4. ActionFactory → Triggers location-based events

### Location-Based Actions
- Actions are defined in `api/Erwin.Games.TreasureIsland.Actions/`
- Automatically triggered when entering locations based on `worldData.json`
- Actions provide dynamic behavior based on game state

### API Communication
- Frontend uses fetch API through service layer
- Backend Azure Functions expose HTTP endpoints
- Game state persisted to Cosmos DB

## Key Domain Knowledge

### Game Mechanics
- Text-based adventure with natural language command parsing
- Player navigates locations, picks up items, interacts with environment
- Save/load functionality with multiple save slots
- AI-enhanced features (command parsing, descriptions, fortune telling)

### Location System
- Locations defined in `api/Data/worldData.json`
- Each location has: description, allowed movements, items, actions
- Movement directions: cardinal (N/S/E/W) and relative (ahead/behind/left/right)
- Location images stored in `public/images/`

## Troubleshooting

### Common Issues
- **CORS errors**: Ensure backend is running and REACT_APP_API_URL is set correctly
- **Build failures**: Check Node.js (18+) and .NET 8.0 SDK versions
- **Database errors**: Verify Cosmos DB credentials in `local.settings.json`
- **Missing world data**: Run UpsertWorldData utility to populate database

## Additional Resources

- Game is based on original DOS game by Richard Erwin (1980s)
- Uses Azure Static Web Apps with integrated Azure Functions
- AI features powered by Azure OpenAI Service
- Images generated with Midjourney AI
