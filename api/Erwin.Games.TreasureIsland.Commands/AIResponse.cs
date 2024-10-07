namespace Erwin.Games.TreasureIsland.Commands
{
    public record AIResponse
    {
        public Choice[]? Choices { get; init; }
        public long Created { get; init; }
        public string? Id { get; init; }
        public string? Model { get; init; }
        public string? Object { get; init; }
        public Usage? Usage { get; init; }
    }

    public record Choice
    {
        public string? FinishReason { get; init; }
        public int Index { get; init; }
        public Message? Message { get; init; }
    }

    public record Message
    {
        public string? Content { get; init; }
        public string? Role { get; init; }
        public object[]? ToolCalls { get; init; }
    }

    public record Usage
    {
        public int CompletionTokens { get; init; }
        public int PromptTokens { get; init; }
        public int TotalTokens { get; init; }
    }
}
