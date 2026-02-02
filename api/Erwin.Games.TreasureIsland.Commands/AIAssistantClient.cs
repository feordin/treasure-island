using Azure;
using Azure.AI.OpenAI.Assistants;
using Azure.Core;
using Azure.AI.OpenAI;
using System.ClientModel;
using System.Security.Cryptography.Xml;
using Erwin.Games.TreasureIsland.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Linq;

namespace Erwin.Games.TreasureIsland.Commands
{
    public class AIAssistantClient : IAIClient
    {
        private readonly string? _apiKey = Environment.GetEnvironmentVariable("AzureAIStudioApiKey");
        private readonly string? _assistantId = Environment.GetEnvironmentVariable("AzureAIStudioAssistantId");
        private readonly string? _endpoint = Environment.GetEnvironmentVariable("AzureAIStudioEndpoint");
        private readonly string? _modelName = Environment.GetEnvironmentVariable("AzureAIModelName");
        private readonly AssistantsClient _assistantsClient;
        private readonly Azure.AI.OpenAI.Assistants.Assistant _assistant;
        private readonly Azure.AI.OpenAI.Assistants.AssistantThread _thread;
        private readonly ILogger<AIAssistantClient> _logger;

        public AIAssistantClient(ILogger<AIAssistantClient> logger)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_assistantId) || string.IsNullOrEmpty(_endpoint))
            {
                throw new Exception("Azure AI Studio environment variables are not set.");
            }

            _assistantsClient = new AssistantsClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));
            _assistant = _assistantsClient.GetAssistant(_assistantId);
            _logger.LogInformation("Recevied assistant details: " + _assistant.Name);
            _thread = _assistantsClient.CreateThread();
        }

        public Task<string?> GetEmbelleshedLocationDescription(string? description)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetFortune()
        {
            throw new NotImplementedException();
        }

        public async Task<string?> ParsePlayerInput(string? input)
        {
            /* 
            var assistantCreationOptions = new Azure.AI.OpenAI.Assistants.AssistantCreationOptions(_modelName)
            {
                Name = "ti-command-interpreter",
                Instructions = @"You are an assistant to a text based adventure game engine.  The player's commands are sent to you, 
                   and you should determine if the command matches on the allowed commands in the game.  Following is the list of allowed commands:
                   ""look"", ""inventory"", ""open"", ""close"", ""take"", ""say"", ""north"", ""south"", ""east"", ""west"", ""up"", ""down"", 
                   ""help"", ""eat"", ""drop"", ""save"", ""load"", ""delete"", ""new"", ""sleep"", ""unlock"", ""lock""

                The following subset  of commands should also be followed by the next word:
                ""open"", ""close"", ""take"", ""say"", ""eat"", ""drop"", ""unlock"", ""lock""
                For example:  take shovel or open door.

                The following commands relate to saving, loading or deleting games:
                ""save"", ""load"", ""delete""
                These should indicate which number that should be saved, loaded or deleted.
                The output should be: ""save 1"" or ""delete 1"".  If no number is identified, acceptable output is simply the single command word.
                For example: ""save""

                The output should be only matched command, for example: ""north""  or the matched command with additional word, for example: ""drop matches""

                If you cannot match the input to one of the commands respond with: ""unknown_command""",
                Tools = {  }
            };
            */

            var message = _assistantsClient.CreateMessage(_thread.Id, MessageRole.User, input);
            var runResult = _assistantsClient.CreateRun(_thread, _assistant);
            // check for RunStatus terminal state
            while (runResult.Value.Status != RunStatus.Completed && 
                   runResult.Value.Status != RunStatus.Cancelled &&
                   runResult.Value.Status != RunStatus.Failed &&
                   runResult.Value.Status != RunStatus.Expired)
            { 
                runResult = await _assistantsClient.GetRunAsync(_thread.Id, runResult.Value.Id);
            }
            
            if (runResult.Value.Status == RunStatus.Completed)
            {
                var messages = _assistantsClient.GetMessages(_thread.Id);
                var content = messages?.Value?.Where(m => m.Role == MessageRole.Assistant).FirstOrDefault()?.ContentItems[0] as MessageTextContent;
                return content?.Text;
            }
            else
            {
                _logger.LogInformation("Run did not complete, status is: " + runResult.Value.Status);
            }

            return "I didn't get that.  Please try again.";
        }

        public Task<string?> Pray()
        {
            throw new NotImplementedException();
        }

        public Task<string?> ParsePlayerInputWithAgent(string? input)
        {
            // Not implemented for this client - use AIChatClient instead
            throw new NotImplementedException("ParsePlayerInputWithAgent is only available in AIChatClient");
        }
    }
}