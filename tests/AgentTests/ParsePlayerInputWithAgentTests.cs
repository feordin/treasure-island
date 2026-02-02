using Xunit;
using Xunit.Abstractions;
using Erwin.Games.TreasureIsland.Commands;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace AgentTests
{
    public class ParsePlayerInputWithAgentTests
    {
        private readonly ITestOutputHelper _output;

        static ParsePlayerInputWithAgentTests()
        {
            // Load environment variables from local.settings.json
            LoadLocalSettings();
        }

        private static void LoadLocalSettings()
        {
            var settingsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..", "..", "api", "local.settings.json");

            var fullPath = Path.GetFullPath(settingsPath);

            if (File.Exists(fullPath))
            {
                var json = File.ReadAllText(fullPath);
                var settings = JsonSerializer.Deserialize<LocalSettings>(json);

                if (settings?.Values != null)
                {
                    foreach (var kvp in settings.Values)
                    {
                        if (Environment.GetEnvironmentVariable(kvp.Key) == null)
                        {
                            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
                        }
                    }
                }
            }
        }

        public ParsePlayerInputWithAgentTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private AIChatClient CreateClient()
        {
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            var logger = new Mock<ILogger<AIChatClient>>();

            // Log any errors to test output
            logger.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback((LogLevel level, EventId eventId, object state, Exception? ex, Delegate formatter) =>
                {
                    _output.WriteLine($"[{level}] {state}");
                    if (ex != null)
                        _output.WriteLine($"Exception: {ex.Message}\n{ex.StackTrace}");
                });

            return new AIChatClient(httpClientFactory.Object, logger.Object);
        }

        [Fact]
        public async Task ParsePlayerInputWithAgent_MovementCommand_ReturnsDirection()
        {
            // Arrange
            var client = CreateClient();
            var input = "I want to go north";

            // Act
            var result = await client.ParsePlayerInputWithAgent(input);

            // Assert
            _output.WriteLine($"Input: '{input}' => Result: '{result}'");
            Assert.NotNull(result);
            Assert.DoesNotContain("unknown_command", result.ToLower());
            Assert.Contains("north", result.ToLower());
        }

        [Fact]
        public async Task ParsePlayerInputWithAgent_TakeCommand_ReturnsItemAction()
        {
            // Arrange
            var client = CreateClient();
            var input = "pick up the shovel";

            // Act
            var result = await client.ParsePlayerInputWithAgent(input);

            // Assert
            _output.WriteLine($"Input: '{input}' => Result: '{result}'");
            Assert.NotNull(result);
            Assert.DoesNotContain("unknown_command", result.ToLower());
            Assert.Contains("take", result.ToLower());
            Assert.Contains("shovel", result.ToLower());
        }

        [Fact]
        public async Task ParsePlayerInputWithAgent_LookCommand_ReturnsLook()
        {
            // Arrange
            var client = CreateClient();
            var input = "look around";

            // Act
            var result = await client.ParsePlayerInputWithAgent(input);

            // Assert
            _output.WriteLine($"Input: '{input}' => Result: '{result}'");
            Assert.NotNull(result);
            Assert.Contains("look", result.ToLower());
        }

        [Fact]
        public async Task ParsePlayerInputWithAgent_ExamineCommand_ReturnsExamine()
        {
            // Arrange
            var client = CreateClient();
            var input = "what is the letter about?";

            // Act
            var result = await client.ParsePlayerInputWithAgent(input);

            // Assert
            _output.WriteLine($"Input: '{input}' => Result: '{result}'");
            Assert.NotNull(result);
            Assert.Contains("examine", result.ToLower());
        }

        [Fact]
        public async Task ParsePlayerInputWithAgent_SaveCommand_ReturnsSave()
        {
            // Arrange
            var client = CreateClient();
            var input = "save my game to slot 2";

            // Act
            var result = await client.ParsePlayerInputWithAgent(input);

            // Assert
            _output.WriteLine($"Input: '{input}' => Result: '{result}'");
            Assert.NotNull(result);
            Assert.Contains("save", result.ToLower());
        }

        [Fact]
        public async Task ParsePlayerInputWithAgent_KillDraculaCommand_ReturnsKill()
        {
            // Arrange
            var client = CreateClient();
            var input = "stake the vampire";

            // Act
            var result = await client.ParsePlayerInputWithAgent(input);

            // Assert
            _output.WriteLine($"Input: '{input}' => Result: '{result}'");
            Assert.NotNull(result);
            // Should return kill or stake command
            Assert.True(result.ToLower().Contains("kill") || result.ToLower().Contains("stake"),
                $"Expected 'kill' or 'stake' but got '{result}'");
        }

        private class LocalSettings
        {
            public Dictionary<string, string>? Values { get; set; }
        }
    }
}
