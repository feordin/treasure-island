using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Erwin.Games.TreasureIsland.Models;
using Microsoft.Extensions.Logging;

namespace Erwin.Games.TreasureIsland.Persistence
{
    public class CosmosDbGameRepository : IGameDataRepository
    {
        private static Lazy<CosmosClient> _lazyClient = new Lazy<CosmosClient>(InitializeCosmosClient);
        private static CosmosClient _cosmosClient => _lazyClient.Value;
        private static readonly string _databaseId = "treasureisland";
        private static readonly string _containerId = "gamedata";
        private readonly ILogger<CosmosDbGameRepository> _logger;
        private Container _container;

        private static CosmosClient InitializeCosmosClient()
        {
            // Perform any initialization here
            var uri = Environment.GetEnvironmentVariable("CosmosDBEndpoint");
            var key = Environment.GetEnvironmentVariable("CosmosDBKey");
            return new CosmosClient(uri, key);
        }


        public CosmosDbGameRepository(ILogger<CosmosDbGameRepository> logger)
        {
            _logger = logger;
            _container = _cosmosClient.GetContainer(_databaseId, _containerId);
        }

        public async Task<SaveGameData?> LoadGameAsync(string? user = null, int id = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(user))
                {
                    user = ClientPrincipal.Instance?.UserDetails;
                }

                var cosmosId = user + "_" + id.ToString();
                ItemResponse<SaveGameData?> response = await _container.ReadItemAsync<SaveGameData?>(cosmosId, new PartitionKey(cosmosId));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Game data not found for user {0} and id {1}", user, id);
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
                gameData.id = cosmosId;
                ItemResponse<SaveGameData> response = await _container.UpsertItemAsync<SaveGameData>(gameData, new PartitionKey(cosmosId));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Unable to save game data for user {0} and id {1}", ClientPrincipal.Instance?.UserDetails, id);
                return false;
            }
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            var cosmosId = ClientPrincipal.Instance?.UserDetails + "_" + id.ToString();

            try
            {
                await _container.DeleteItemAsync<SaveGameData>(cosmosId, new PartitionKey(cosmosId));
                cosmosId = ClientPrincipal.Instance?.UserDetails + "_history_" + id.ToString();
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
                ItemResponse<CommandHistory?> response = await _container.ReadItemAsync<CommandHistory?>(cosmosId, new PartitionKey(cosmosId));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Game history data not found for user {0} and id {1}", user, id);
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
                ItemResponse<CommandHistory> response = await _container.UpsertItemAsync<CommandHistory>(commandHistory, new PartitionKey(cosmosId));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, "Unable to save game history data for user {0} and id {1}", ClientPrincipal.Instance?.UserDetails, id);
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