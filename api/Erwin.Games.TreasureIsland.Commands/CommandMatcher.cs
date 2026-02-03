namespace Erwin.Games.TreasureIsland.Commands
{
    /// <summary>
    /// Fast-path command matcher that bypasses AI for direct command matches.
    /// Uses O(1) dictionary lookup for instant command resolution.
    /// </summary>
    public static class CommandMatcher
    {
        // Direct command mappings (input -> output command)
        private static readonly Dictionary<string, string> _exactMatches = new(StringComparer.OrdinalIgnoreCase)
        {
            // Movement - cardinal
            { "n", "north" }, { "north", "north" }, { "go north", "north" },
            { "s", "south" }, { "south", "south" }, { "go south", "south" },
            { "e", "east" }, { "east", "east" }, { "go east", "east" },
            { "w", "west" }, { "west", "west" }, { "go west", "west" },
            { "u", "up" }, { "up", "up" }, { "go up", "up" },
            { "d", "down" }, { "down", "down" }, { "go down", "down" },

            // Movement - relative
            { "left", "left" }, { "go left", "left" }, { "turn left", "left" },
            { "right", "right" }, { "go right", "right" }, { "turn right", "right" },
            { "ahead", "ahead" }, { "go ahead", "ahead" }, { "forward", "ahead" },
            { "go forward", "ahead" }, { "straight", "ahead" }, { "go straight", "ahead" },
            { "behind", "behind" }, { "back", "behind" }, { "go back", "behind" },
            { "turn around", "behind" }, { "backward", "behind" },

            // Exploration
            { "l", "look" }, { "look", "look" }, { "look around", "look" }, { "where am i", "look" },
            { "inventory", "inventory" }, { "i", "inventory" },

            // System
            { "help", "help" }, { "h", "help" }, { "hint", "help" }, { "?", "help" },
            { "new", "new" }, { "start", "new" }, { "restart", "new" }, { "begin", "new" },
            { "save", "save" }, { "save game", "save" },
            { "embellish", "embellish" },

            // Actions
            { "sleep", "sleep" }, { "go to sleep", "sleep" }, { "nap", "sleep" },
            { "rest", "rest" }, { "take a break", "rest" }, { "relax", "rest" },
            { "dig", "dig" }, { "dig hole", "dig" }, { "excavate", "dig" },
            { "pray", "pray" }, { "worship", "pray" },
            { "fortune", "fortune" }, { "tell fortune", "fortune" }, { "read fortune", "fortune" },
            { "swim", "swim" }, { "swim across", "swim" },
            { "drink", "drink" }, { "drink water", "drink" },
            { "steal", "steal" }, { "rob", "steal" }, { "rob bank", "steal" },
            { "kill", "kill" }, { "attack", "kill" }, { "stake", "kill" }, { "slay", "kill" },
        };

        // Commands that take a parameter (used for pattern matching)
        private static readonly HashSet<string> _parameterCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            "take", "get", "grab", "pick up", "pickup", "collect",
            "drop", "put down", "leave", "discard",
            "examine", "read", "inspect", "look at", "check", "study",
            "buy", "purchase",
            "pawn", "sell",
            "light", "burn", "ignite",
            "fill", "fill up", "refill",
            "rub", "use",
            "save", "load", "delete", "kill"
        };

        // Command verb mappings for parameter extraction
        // NOTE: "leave" and "exit" removed - they need location context handled by AI
        private static readonly Dictionary<string, string> _verbMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            { "get", "take" }, { "grab", "take" }, { "pick up", "take" }, { "pickup", "take" }, { "collect", "take" },
            { "put down", "drop" }, { "discard", "drop" },
            { "read", "examine" }, { "inspect", "examine" }, { "look at", "examine" }, { "check", "examine" }, { "study", "examine" },
            { "purchase", "buy" },
            { "sell", "pawn" },
            { "burn", "light" }, { "ignite", "light" }, { "set fire to", "light" },
            { "fill up", "fill" }, { "refill", "fill" },
            { "use", "rub" },
            { "load game", "load" }, { "restore", "load" },
            { "save game", "save" },
            { "delete save", "delete" }, { "remove save", "delete" },
            { "stake", "kill" }, { "attack", "kill" }, { "slay", "kill" },
        };

        // Phrases that should NOT be fast-path matched (need location context from AI)
        private static readonly string[] _needsLocationContext = new[]
        {
            "leave", "exit", "enter", "go to", "go into", "walk to", "walk into",
            "go down the", "go up the", "go through", "go across", "go around",
            "walk down", "walk up", "head to", "head down", "head up"
        };

        /// <summary>
        /// Attempts to match the input to a known command without AI.
        /// Returns null if no direct match found (AI should be used).
        /// </summary>
        public static string? TryMatchCommand(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Trim();

            // 0. Check if input needs location context (let AI handle it)
            foreach (var phrase in _needsLocationContext)
            {
                if (input.StartsWith(phrase, StringComparison.OrdinalIgnoreCase))
                    return null; // Needs AI with location context
            }

            // 1. Check exact matches first (fastest path)
            if (_exactMatches.TryGetValue(input, out var exactMatch))
                return exactMatch;

            // 2. Try to match "verb + parameter" patterns
            var result = TryMatchParameterCommand(input);
            if (result != null)
                return result;

            return null; // No match - needs AI
        }

        private static string? TryMatchParameterCommand(string input)
        {
            // Try each verb mapping
            foreach (var (verb, command) in _verbMappings)
            {
                if (input.StartsWith(verb + " ", StringComparison.OrdinalIgnoreCase))
                {
                    var parameter = input.Substring(verb.Length + 1).Trim();
                    parameter = NormalizeParameter(parameter);
                    if (!string.IsNullOrEmpty(parameter))
                        return $"{command} {parameter}";
                }
            }

            // Try base commands directly (e.g., "take shovel", "examine letter")
            foreach (var cmd in new[] { "take", "drop", "examine", "buy", "pawn", "light", "fill", "rub", "save", "load", "delete", "kill" })
            {
                if (input.StartsWith(cmd + " ", StringComparison.OrdinalIgnoreCase))
                {
                    var parameter = input.Substring(cmd.Length + 1).Trim();
                    parameter = NormalizeParameter(parameter);
                    if (!string.IsNullOrEmpty(parameter))
                        return $"{cmd} {parameter}";
                }
            }

            return null;
        }

        private static string NormalizeParameter(string parameter)
        {
            // Remove common articles
            var articles = new[] { "the ", "a ", "an ", "some " };
            foreach (var article in articles)
            {
                if (parameter.StartsWith(article, StringComparison.OrdinalIgnoreCase))
                {
                    parameter = parameter.Substring(article.Length);
                    break;
                }
            }

            // Convert to camelCase for multi-word items
            if (parameter.Contains(' '))
            {
                var words = parameter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                parameter = words[0].ToLower() + string.Concat(words.Skip(1).Select(w =>
                    char.ToUpper(w[0]) + w.Substring(1).ToLower()));
            }

            return parameter.ToLower();
        }
    }
}
