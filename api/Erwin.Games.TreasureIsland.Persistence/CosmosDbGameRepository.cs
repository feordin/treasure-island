using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Erwin.Games.TreasureIsland.Models;
using Microsoft.Extensions.Logging;
using Erwin.Games.TreasureIsland.Commands;

namespace Erwin.Games.TreasureIsland.Persistence
{
    public class CosmosDbGameRepository : IGameDataRepository
    {
        private static readonly string _databaseId = "treasureisland";
        private static readonly string _containerId = "gamedata";
        private CosmosClient _cosmosClient;
        private readonly ILogger<CosmosDbGameRepository> _logger;
        private Container _container;
        private IAIClient _aiClient;


        public CosmosDbGameRepository(ILogger<CosmosDbGameRepository> logger, IHttpClientFactory httpClientFactory, IAIClient aiClient)
        {
            _logger = logger;
            
            var uri = Environment.GetEnvironmentVariable("CosmosDBEndpoint");
            var key = Environment.GetEnvironmentVariable("CosmosDBKey");

            _cosmosClient = new CosmosClient(uri, key, new CosmosClientOptions() { HttpClientFactory = httpClientFactory.CreateClient });
            _container = _cosmosClient.GetContainer(_databaseId, _containerId);
            _aiClient = aiClient;
        }

        public async Task<SaveGameData?> LoadGameAsync(string? user = null, int id = 0)
        {
            if (string.IsNullOrEmpty(user))
            {
                user = ClientPrincipal.Instance?.UserDetails;
            }

            var cosmosId = user + "_" + id.ToString();

            return await LoadGameAsync(cosmosId);
        }

        public async Task<SaveGameData?> LoadGameAsync(string cosmosId)
        {
            try
            {
                ItemResponse<SaveGameData?> response = await _container.ReadItemAsync<SaveGameData?>(cosmosId, new PartitionKey(cosmosId));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Game data not found for game id: {0}", cosmosId);
                return null;
            }
        }

        public async Task<bool> SaveGameAsync(SaveGameData? gameData, int id = 0)
        {
            if (gameData == null)
            {
                return false;
            }

            try
            {
                var cosmosId = ClientPrincipal.Instance?.UserDetails + "_" + id.ToString();
                return await SaveGameAsync(gameData, cosmosId);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Unable to save game data for user {0} and id {1}", ClientPrincipal.Instance?.UserDetails, id);
                return false;
            }
        }

        public async Task<bool> SaveGameAsync(SaveGameData? gameData, string cosmosId)
        {
            if (gameData == null)
            {
                return false;
            }

            try
            {
                gameData.id = cosmosId;
                ItemResponse<SaveGameData> response = await _container.UpsertItemAsync<SaveGameData>(gameData, new PartitionKey(cosmosId));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Unable to save game data for game id {0}", cosmosId);
                return false;
            }
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            var cosmosId = ClientPrincipal.Instance?.UserDetails + "_" + id.ToString();

            return await DeleteGameAsync(cosmosId);
        }

        public async Task<bool> DeleteGameAsync(string id)
        {
            var cosmosId = id;

            try
            {
                await _container.DeleteItemAsync<SaveGameData>(cosmosId, new PartitionKey(cosmosId));
                var gameIdTokens = id.Split('_');
                var commandHistoryId = gameIdTokens[0] + "_history_" + gameIdTokens[1];
                cosmosId = commandHistoryId;
                await _container.DeleteItemAsync<SaveGameData>(cosmosId, new PartitionKey(cosmosId));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Unable to delete game data for user {0} and id {1}", ClientPrincipal.Instance?.UserDetails, id);
                return false;
            }
        }


        public async Task<WorldData?> LoadWorldDataAsync()
        {
            try
            {
                ItemResponse<WorldData?> response = await _container.ReadItemAsync<WorldData?>("treasure-island", new PartitionKey("treasure-island"));
                foreach(var location in response.Resource?.Locations ?? new List<Location>())
                {
                    location.AiClient = _aiClient;
                }
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Unable to load world data.");
                return null;
            }
        }

        public async Task<CommandHistory?> LoadCommandHistoryAsync(string? user, int id)
        {
            try
            {
                if (string.IsNullOrEmpty(user))
                {
                    user = ClientPrincipal.Instance?.UserDetails;
                }

                var cosmosId = user + "_history_" + id.ToString();
                return await LoadCommandHistoryAsync(cosmosId);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Game history data not found for user {0} and id {1}", user, id);
                return null;
            }
        }

        public async Task<CommandHistory?> LoadCommandHistoryAsync(string cosmosId)
        {
            try
            {
                ItemResponse<CommandHistory?> response = await _container.ReadItemAsync<CommandHistory?>(cosmosId, new PartitionKey(cosmosId));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Game history data not found for history id {0}", cosmosId);
                return null;
            }
        }

        public async Task<bool> SaveCommandHistoryAsync(CommandHistory? commandHistory, string? user, int id)
        {
            if (commandHistory == null)
            {
                return false;
            }

            try
            {
                if (string.IsNullOrEmpty(user))
                {
                    user = ClientPrincipal.Instance?.UserDetails;
                }

                var cosmosId = user + "_history_" + id.ToString();
                commandHistory.id = cosmosId;
                return await SaveCommandHistoryAsync(commandHistory, cosmosId);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Unable to save game history data for user {0} and id {1}", ClientPrincipal.Instance?.UserDetails, id);
                return false;
            }
        }

        public async Task<bool> SaveCommandHistoryAsync(CommandHistory? commandHistory, string cosmosId)
        {
            if (commandHistory == null)
            {
                return false;
            }

            try
            {
                commandHistory.id = cosmosId;
                ItemResponse<CommandHistory> response = await _container.UpsertItemAsync<CommandHistory>(commandHistory, new PartitionKey(cosmosId));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Unable to save game history data for history id {0}", cosmosId);
                return false;
            }
        }

        public async Task<IReadOnlyCollection<SaveGameData>?> GetAllSavedGamesAsync(string? user)
        {
            try {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.Player = @user")
                    .WithParameter("@user", user);

                var iterator = _container.GetItemQueryIterator<SaveGameData>(query);
                var results = new List<SaveGameData>();

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    results.AddRange(response.ToList());
                }

                return results;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Unable to load game list for user {0}", ClientPrincipal.Instance?.UserDetails);
                return null;
            }
        }

        public async Task<bool> AddToGameHistory(string? command, string? response, string? user, int gameNumber)
        {
            if (command != null && response != null && user != null)
            {
                var history = await LoadCommandHistoryAsync(user, gameNumber);
                if (history == null)
                {
                    history = new CommandHistory();
                }
                history.AddHistory(command, response);
                await SaveCommandHistoryAsync(history, user, gameNumber);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}