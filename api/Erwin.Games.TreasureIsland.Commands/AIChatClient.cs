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
                                ""startup"", ""look"",""inventory"", ""open"",""close"",""take"",""say"",""north"",""south"",""east"",""west"",""up"",""down"",""help"", ""eat"",""drink"", ""drop"",""save"",""load"",""delete"",""new"",""sleep"",""unlock"",""lock"",""steal"",""borrow""
                                        
                                The following subset of commands should also be followed by the next word:
                                ""look"",""open"",""close"",""take"",""say"",""eat"",""drink"", ""drop"",""unlock"",""lock"",""steal"",""borrow""
                                For example:  ""take shovel"" or ""open door"".The game needs to know which item is being acted upon.

                                The following commands relate to starting, saving, loading or deleting games:
                                ""new"",""save"",""load"",""delete""
                                Except for the ""new"" command, these should indicate which number that should be saved, loaded or deleted.The output should be: ""save 1"" or ""delete 1"".If no number is identified, acceptable output is simply the single command word.For example: ""save""
                                
                                The following subset of commands: ""north"", ""south"", ""east"", ""west"", ""up"", and ""down""
                                are related to moving.  These should be the output if the user mentions something that indicates they want to move or go somewhere.

                                Any input similar to ""start a new game"" or ""start over"" should return ""new"".

                                The output should be only the matched command, for example:  ""north""  or the matched command with additional word, for example:  ""drop matches"".  The consumer of the ouput is the game engine,
                                so the output should be in a format that the game engine can understand, which is the matched command text without any additional explanation.

                                If you cannot match the input to one of the commands respond with: unknown_command, and then add some helpful hints to the player about what commands might be available.  Along with a fun description story or quote to brighten their day."
            ;
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