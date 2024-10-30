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

namespace Erwin.Games.TreasureIsland.Commands
{
    public class AIChatClient : IAIClient
    {
        private readonly string? _apiKey = Environment.GetEnvironmentVariable("AzureAIStudioApiKey");
        private readonly string? _endpoint = Environment.GetEnvironmentVariable("AzureAIStudioEndpoint");
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

            _systemPromptText = @"You are an assistant to a text-based adventure game engine.  The player's commands are sent to you, and you should determine if the command matches on the allowed commands in the game. Your goal is to
                                determine the player's intent.  They may not enter the exact command, but may enter something which indicates a similar intent.  If the user does enter an exact command, you should return that command.
                                Following is the complete list of allowed commands:
                                ""startup"", ""look"",""inventory"", ""open"",""close"",""take"",""say"",""north"",""south"",""east"",""west"",""up"",""down"",""ahead"",""behind"", ""left"", ""right"", ""help"", ""eat"",""drink"", ""drop"",""save"",""load"",""delete"",""new"",""sleep"",""unlock"",""lock"",""steal"",""borrow"",""embellish"", ""examine"", ""read"", ""buy"", ""pray"", ""pawn"", ""fortune""
                                        
                                The following subset of commands should also be followed by the next word, or the word associate with the command:
                                ""look"",""open"",""close"",""take"",""say"",""eat"",""drink"", ""drop"",""unlock"",""lock"",""steal"",""borrow"", ""examine"", ""read"", ""buy"", ""pawn""
                                For example:  if the user input is: ""I want to pick up the shovel"", then your response would be: ""take shovel"". The game needs to know which item is being acted upon.

                                The following commands relate to starting, saving, loading or deleting games:
                                ""new"",""save"",""load"",""delete""
                                Except for the ""new"" command, these should indicate which number that should be saved, loaded or deleted.The output should be: ""save 1"" or ""delete 1"".If no number is identified, acceptable output is simply the single command word.For example: ""save""
                                
                                The following subset of commands: ""north"", ""south"", ""east"", ""west"", ""up"", ""down"", ""left"", ""right"", ""ahead"", and ""behind""
                                are related to moving.  These should be the output if the user mentions something that indicates they want to move or go somewhere. 
                                if the user indicates going forward or onward,  return ""ahead""
                                If the user indicates going back or returning, return ""behaind""

                                Any input similar to ""start a new game"" or ""start over"" should return ""new"".

                                If the user asks to have their fortune read.  return ""fortune""

                                To distinguish between ""look"" and ""examine"" if the user is trying to get look at or in a specific object, return ""examine"" followed by the word for the object.  If they are just looking around, return ""look""

                                The output should be only the matched command, for example:  ""north""  or the matched command with additional word, for example:  ""drop matches"".  The consumer of the ouput is the game engine,
                                so the output should be in a format that the game engine can understand, which is the matched command text without any additional explanation.

                                If you cannot match the input to one of the commands respond with: unknown_command, and then add some helpful hints to the player about what commands might be available.  Along with a fun description story or quote to brighten their day."
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
                temperature = 0.9,
                top_p = 0.95,
                max_tokens = 16384,
                stream = false
            };

            return await GetResponse(payload);

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