using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Erwin.Games.TreasureIsland
{
    public class GetSpeechToken
    {
        private readonly ILogger<GetSpeechToken> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _speechKey;
        private readonly string? _speechRegion;

        public GetSpeechToken(ILogger<GetSpeechToken> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _speechKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
            _speechRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");
        }

        [Function("GetSpeechToken")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("GetSpeechToken function triggered");

            if (string.IsNullOrEmpty(_speechKey) || string.IsNullOrEmpty(_speechRegion))
            {
                _logger.LogError("Speech service credentials not configured");
                return new StatusCodeResult(500);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var tokenEndpoint = $"https://{_speechRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken";

                var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _speechKey);
                request.Content = new StringContent("");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();

                    return new OkObjectResult(new
                    {
                        token = token,
                        region = _speechRegion
                    });
                }
                else
                {
                    _logger.LogError("Failed to get speech token: {StatusCode}", response.StatusCode);
                    return new StatusCodeResult((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting speech token");
                return new StatusCodeResult(500);
            }
        }
    }
}
