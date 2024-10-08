// saveGameData.ts

export interface SaveGameData {
    player?: string;
    score?: number;
    currentLocation?: string;
    currentDateTime?: string; // ISO 8601 date string
    inventory?: string[];
    health?: number;
    history?: CommandHistory;
    locationChanges?: LocationChange[];
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