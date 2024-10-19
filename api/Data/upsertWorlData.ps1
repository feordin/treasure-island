param (
    [string]$endpoint = $null,
    [string]$key = $null,
    [string]$database = "treasureisland",
    [string]$container = "gamedata",
    [string]$filePath = "worldData.json",
    [string]$settingsFilePath = "..\local.settings.json"
)

# Install Azure Cosmos DB module if not already installed
if (-not (Get-Module -ListAvailable -Name Az.CosmosDB)) {
    Install-Module -Name Az.CosmosDB -Force -Scope CurrentUser
}

Import-Module Az.CosmosDB

# Read the local settings file if endpoint or key is null
if (-not $endpoint -or -not $key) {
    $settings = Get-Content -Path $settingsFilePath -Raw | ConvertFrom-Json
    if (-not $endpoint) {
        $endpoint = $settings.Values.CosmosDBEndpoint
    }
    if (-not $key) {
        $key = $settings.Values.CosmosDBKey
    }
}

# Use environment variables if available
if (-not $endpoint) {
    $endpoint = $env:COSMOSDB_ENDPOINT
}
if (-not $key) {
    $key = $env:COSMOSDB_KEY
}

Write-Output "Endpoint: $endpoint"
Write-Output "Key: $key"

if ($endpoint -and $key) 
{
    # Read the JSON file
    $jsonContent = Get-Content -Path $filePath -Raw | ConvertFrom-Json

    # Upsert the document
    $cosmosDbContext = New-AzCosmosDBContext -AccountEndpoint $endpoint -AccountKey $key
    $cosmosDbContext | New-AzCosmosDBSqlDocument -DatabaseName $database -ContainerName $container -DocumentBody $jsonContent
    Write-Output "Document upserted successfully to database '$database' and container '$container'."
}
else
{
	Write-Output "No endpoint or key provided."
}
