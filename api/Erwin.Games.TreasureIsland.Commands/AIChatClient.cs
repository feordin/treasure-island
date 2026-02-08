using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.AI.OpenAI;
using System.Text;
using System.ClientModel;
using Azure;
using OpenAI;
using static System.Net.Mime.MediaTypeNames;
using OpenAI.Chat;
using Azure.AI.Inference;
using Azure.AI.Projects;
using Azure.Identity;
using Erwin.Games.TreasureIsland.Models;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class AIChatClient : IAIClient
    {
        private readonly string? _apiKey = Environment.GetEnvironmentVariable("AzureAIStudioApiKey");
        private readonly string? _endpoint = Environment.GetEnvironmentVariable("AzureAIStudioEndpoint");
        private readonly string? _agentConnectionString = Environment.GetEnvironmentVariable("AgentConnectionString")
            ?? "westus.api.azureml.ms;3b0cd1dd-1e41-4f35-bd69-485b59588bbf;feordin-ai-studio;treasureisland";
        private readonly string? _agentId = Environment.GetEnvironmentVariable("AgentId")
            ?? "asst_B0MuLNtSJpg1PBMdyWrsZ3B6";
        private readonly string _systemPromptText;
        private readonly string _systemLocationText;
        private readonly string _systemFortuneText;
        private readonly string _systemPrayText;
        private readonly HttpClient _client;
        private readonly ILogger<AIChatClient> _logger;

        public AIChatClient(IHttpClientFactory httpClientFactory, ILogger<AIChatClient> logger)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(_apiKey) ||string.IsNullOrEmpty(_endpoint))
            {
                throw new Exception("Azure AI Studio environment variables are not set.");
            }

            _client = httpClientFactory.CreateClient();

            // Optimized minimal prompt for fast command parsing
            _systemPromptText = @"Parse text adventure commands. Output ONLY the command, nothing else.

COMMANDS: north,south,east,west,up,down,left,right,ahead,behind,look,take,drop,examine,buy,pawn,save,load,delete,new,help,sleep,rest,dig,pray,fortune,swim,drink,fill,rub,light,kill,steal,borrow,wish,signal,embellish,inventory,open

RULES:
- Movement: forward/onward=ahead, back/return=behind
- With items: take/get/grab X=take X, look at/read/inspect X=examine X
- Save/load/delete need slot number if given: ""save to slot 2""=save 2
- Location context [LOCATION CONTEXT] shows exits, description, and current location
- ""enter X""/""go to X"": find direction leading to X from exits, return that direction
- ""exit X""/""leave X"": if player is AT location X, return ""behind""
- ""exit""/""leave"" alone: return ""behind""
- Descriptive movement (""go down the hill"", ""walk up the path""): match description to find which direction, return that direction
- Bank: ""borrow""/""loan""/""get loan""=borrow (for loans), ""steal""/""rob""=steal (for theft). Do NOT confuse these.
- Wish: ""wish for X""/""make a wish""=wish X (valid: money,treasure,food,help,rescue,escape,safety)
- Signal: ""light fire""/""build fire""/""signal fire""/""wait for rescue""/""hope for rescue""/""call for help""=signal
- Open: ""open safe 7-23-42""=open safe 7-23-42, normalize combination numbers to dash-separated format
- Unknown input: respond ""unknown_command"" with brief hint"
            ;

            _systemLocationText = @"You are an AI assistant that helps generate interesting game location descriptions given a base starting point.  Keep important details like relative locations to other locations the same, but make the description more detailed and fun.
                                    Keep in mind the theme of the game.  It is a text based adventure game called treasure island, set in the 17th century Caribbean. Also, keep the description brief.
                                    If the input conainst the following phrase: 'You see the following items:' Use that list of items incorporated into your response.
                                    If the input does not conaint the phrase 'You see the following items:', do not mention any items.";

            _systemFortuneText = @"You are an AI assistant that helps generate interesting fortunes for the player.  Keep the fortune brief and fun.  The player is looking for a little bit of fun and a little bit of insight.  Keep the fortune light and positive.
                                   Some things useful for the player to learn: it is possible to die in a hot air balloon, donuts are useful on Treasure Island, the trash pit has something valuable, there is a pearl in a lagoon, and a genie in a lamp.  Don't make any of that information too obvious.";
            _systemPrayText = @"You are an AI assistant that helps generate interesting answers to prayers for the player.  The answer should come in the form of a vision, which is brief and fun.  The player is looking for a little bit of fun and a little bit of insight.  Use symbols and images rather than stating anything directly.
                                Some things useful for the player to learn: melting ice to form a riven will be helpful on the island, cannibals like donuts, dracula lurks at night, matches are damaged if they get wet.";
        }

        public async Task<string?> GetEmbelleshedLocationDescription(string? description)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var payload = new
            {
                messages = new object[]
                {
                  new { role = "system", content = new object[] { new { type = "text", text = _systemLocationText } } },
                  new { role = "user", content = new object[] { new { type = "text", text = description } } }
                },
                examples = new object[]
                {
                    new {
                        input = "You are at the post office.  The only movement you can make is back to way you came to the west.  You see the following items: letter",
                        output = "The building itself is a simple, single-story structure constructed from coral stone, its whitewashed walls reflecting the harsh Caribbean sunlight. A small, shaded porch offers respite from the heat, where patrons can wait their turn. Inside, the air is cool and dim, the only light filtering through the open doorway and a few small, barred windows. The postmaster, a stern-faced man in a sweat-stained linen shirt, presides over a cluttered desk piled high with letters, parcels, and official documents. He meticulously sorts through the mail, his quill scratching across a ledger as he records each item.  The scent of sealing wax and parchment hangs heavy in the air. A motley crew of customers fills the small space. A wealthy plantation owner waits impatiently to send news of his sugar crop to London. A sailor, fresh off a voyage from Jamaica, eagerly collects a letter from his sweetheart. A nervous young clerk delivers a packet of official dispatches from the Governor's mansion. The walls are adorned with a jumble of notices and proclamations. A tattered map of the Caribbean hangs beside a list of postal rates to far-flung destinations like Boston and Bristol. A royal decree announces the latest regulations on trade and shipping. Despite its humble appearance, the post office is a hub of activity, a vital artery in the flow of information and commerce that binds the colony to the mother country. Here, news of wars and peace, fortunes made and lost, births and deaths, all pass through the hands of the postmaster, connecting lives across vast distances.  There is a letter waiting for you here."
                    }
                },
                temperature = 0.8,
                top_p = 0.95,
                max_tokens = 16384,
                stream = false
            };

            return await GetResponse(payload);
        }

        public async Task<string?> GetFortune()
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var payload = new
            {
                messages = new object[]
                {
                  new { role = "system", content = new object[] { new { type = "text", text = _systemFortuneText } } },
                  new { role = "user", content = new object[] { new { type = "text", text = "Tell me my fortune!" } } }
                },
                temperature = 0.6,
                top_p = 0.95,
                max_tokens = 16384,
                stream = false
            };

            return await GetResponse(payload);
        }

        public async Task<string?> Pray()
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var payload = new
            {
                messages = new object[]
                {
                  new { role = "system", content = new object[] { new { type = "text", text = _systemPrayText } } },
                  new { role = "user", content = new object[] { new { type = "text", text = "I bow my head and pray for guidance." } } }
                },
                temperature = 0.6,
                top_p = 0.95,
                max_tokens = 16384,
                stream = false
            };

            return await GetResponse(payload);
        }

        public async Task<string?> GetPlayerFortune(string? description)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var payload = new
            {
                messages = new object[]
                {
                  new { role = "system", content = new object[] { new { type = "text", text = _systemLocationText } } },
                  new { role = "user", content = new object[] { new { type = "text", text = description } } }
                },
                temperature = 0.8,
                top_p = 0.95,
                max_tokens = 16384,
                stream = false
            };

            return await GetResponse(payload);
        }

        public async Task<string?> ParsePlayerInput(string? input)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var payload = new
            {
                messages = new object[]
                {
                  new { role = "system", content = new object[] { new { type = "text", text = _systemPromptText } } },
                  new { role = "user", content = new object[] { new { type = "text", text = input } } }
                },
                temperature = 0.3,  // Lower temperature for deterministic parsing
                top_p = 0.95,
                max_tokens = 256,   // Commands are short, no need for large output
                stream = false
            };

            return await GetResponse(payload);

        }

        public async Task<string?> ParsePlayerInputWithAgent(string? input, Location? currentLocation, SaveGameData? saveGame)
        {
            // Build contextual input with location information
            string contextualInput = input ?? "";

            if (currentLocation != null && saveGame != null)
            {
                var movements = currentLocation.GetCurrentMovements(saveGame);
                var exits = new List<string>();

                foreach (var movement in movements)
                {
                    if (movement.Direction != null && movement.Direction.Length > 0 && movement.Destination != null)
                    {
                        exits.Add($"{movement.Direction[0]} leads to {movement.Destination}");
                    }
                }

                // Build context with location description for better understanding
                var contextParts = new List<string>
                {
                    $"Player is at: {currentLocation.Name}",
                    $"Player is facing: {saveGame.Facing ?? "north"}"
                };

                // Include brief location description for contextual phrases like "down the hill"
                if (!string.IsNullOrEmpty(currentLocation.Description))
                {
                    // Truncate to first 200 chars to keep tokens low
                    var desc = currentLocation.Description.Length > 200
                        ? currentLocation.Description.Substring(0, 200) + "..."
                        : currentLocation.Description;
                    contextParts.Add($"Description: {desc}");
                }

                if (exits.Count > 0)
                {
                    contextParts.Add($"Available exits: {string.Join("; ", exits)}");
                }

                contextualInput = $@"[LOCATION CONTEXT]
                        {string.Join("\n", contextParts)}

                        [PLAYER INPUT]
                        {input}";
            }

            // Use fast direct endpoint instead of slow agent API
            return await ParsePlayerInput(contextualInput);
        }

        public async Task<string?> ParsePlayerInputWithAgent(string? input)
        {
            // Use fast direct endpoint call instead of agent API for speed
            return await ParsePlayerInput(input);
        }

        // Kept for reference - slower agent-based implementation
        private async Task<string?> ParsePlayerInputWithAgentSlow(string? input)
        {
            try
            {
                // DefaultAzureCredential supports multiple auth methods:
                // 1. Environment variables (AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_TENANT_ID) for service principal
                // 2. Managed Identity when running in Azure
                // 3. Azure CLI credential (az login) for local development
                AgentsClient client = new AgentsClient(_agentConnectionString, new DefaultAzureCredential());

                Response<Agent> agentResponse = await client.GetAgentAsync(_agentId);
                Agent agent = agentResponse.Value;

                Response<AgentThread> threadResponse = await client.CreateThreadAsync();
                AgentThread thread = threadResponse.Value;

                Response<ThreadMessage> messageResponse = await client.CreateMessageAsync(
                    thread.Id,
                    MessageRole.User,
                    input ?? "");

                Response<ThreadRun> runResponse = await client.CreateRunAsync(
                    thread.Id,
                    agent.Id);
                ThreadRun run = runResponse.Value;

                // Poll until the run reaches a terminal status
                do
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    runResponse = await client.GetRunAsync(thread.Id, runResponse.Value.Id);
                }
                while (runResponse.Value.Status == RunStatus.Queued
                    || runResponse.Value.Status == RunStatus.InProgress);

                Response<PageableList<ThreadMessage>> messagesResponse = await client.GetMessagesAsync(thread.Id);
                IReadOnlyList<ThreadMessage> messages = messagesResponse.Value.Data;

                // Get the assistant's response (first message from assistant role)
                foreach (ThreadMessage threadMessage in messages)
                {
                    if (threadMessage.Role == MessageRole.Agent)
                    {
                        foreach (MessageContent contentItem in threadMessage.ContentItems)
                        {
                            if (contentItem is MessageTextContent textItem)
                            {
                                _logger.LogInformation("Agent response: " + textItem.Text);
                                return textItem.Text;
                            }
                        }
                    }
                }

                _logger.LogWarning("No response from agent");
                return "unknown_command";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Foundry agent");
                return "unknown_command";
            }
        }

        private async Task<string?> GetResponse(dynamic payload)
        {
            var response = await _client.PostAsync(_endpoint, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
            
            if (response.IsSuccessStatusCode)
            {
                var chatResponse = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<AIResponse>(chatResponse);
                _logger.LogInformation("AI Chat response: " + responseData.Choices?[0]?.Message?.Content);
                return responseData.Choices?[0]?.Message?.Content;
            }
            else
            {
                _logger.LogError("AI Chat failed with status code: " + response.StatusCode);
                return "unknown_command";
            }
        }
    }
}