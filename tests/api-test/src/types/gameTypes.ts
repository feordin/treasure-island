// Game data types from frontend
export interface SaveGameData {
    player?: string;
    score?: number;
    currentLocation?: string;
    currentDateTime?: string; // ISO 8601 date string
    inventory?: string[];
    health?: number;
    commandHistory?: CommandHistory;
    locationChanges?: LocationChange[];
    aiEmbelleshedDescriptions: boolean;
    events?: Event[];
    money?: number;
    facing?: string;
}

export interface CommandHistory {
    command?: string[];
    response?: string[];
}

export interface LocationChange {
    name?: string;
    itemsAdded?: string[];
    itemsRemoved?: string[];
    thingsOpened?: string[];
    thingsClosed?: string[];
    changeTime: string; // ISO 8601 date string
}

export interface ProcessCommandResponse {
    message?: string;
    saveGameData?: SaveGameData;
    imageFilename?: string;
    locationDescription?: string;
    savedGames?: SaveGameData[];
}

export interface CommandRequest {
    command?: string;
    saveGameData?: SaveGameData;
}

export interface Event {
    name?: string;
    description?: string;
    eventDate?: string;
}

// Test-specific types
export interface TestResult {
    name: string;
    category: string;
    passed: boolean;
    duration: number;
    error?: string;
    details?: Record<string, unknown>;
}

export interface TestSuite {
    name: string;
    tests: TestResult[];
    passed: number;
    failed: number;
    skipped: number;
}

export interface TestReport {
    runId: string;
    startTime: Date;
    endTime: Date;
    apiUrl: string;
    summary: {
        totalTests: number;
        passed: number;
        failed: number;
        skipped: number;
    };
    suites: TestSuite[];
}

export interface TestConfig {
    apiUrl: string;
    verbose: boolean;
    categories: string[];
    timeoutMs: number;
    delayBetweenRequests: number;
    retryCount: number;
    outputDir: string;
}

// Location graph types
export interface LocationNode {
    name: string;
    description?: string;
    movements: Map<string, string>;
    items: string[];
    actions: string[];
    isDeathTrap: boolean;
}

export interface WorldData {
    Locations: Location[];
    Items: Item[];
    GlobalCommands?: string[];
}

export interface Location {
    Name: string;
    Description: string;
    Items?: string[];
    AllowedMovements?: Movement[];
    Actions?: string[];
    Help?: string;
    image?: string;
}

export interface Movement {
    Direction: string[];
    Destination: string;
    TimeToMove?: number;
}

export interface Item {
    Name: string;
    Description: string;
    IsTakeable?: boolean;
    PointValue?: number;
}
