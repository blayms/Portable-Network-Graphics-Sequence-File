using System.Globalization;
using System.Text.RegularExpressions;

namespace Blayms.PNGS.Constructor
{
    internal static class CommandParser
    {
        public static readonly Random Random = new Random();
        public static CommandBase? RootCommand = null;
        public static void Run(string[] args)
        {
            if (args.Length > 0)
            {
                ProcessLine(args[0]);
            }
            while (true)
            {
                string? line = Console.ReadLine()?.Trim();
                ProcessLine(line);
            }
        }

        public static void ProcessLine(string? line)
        {
            ResetStringArgsBuffer();
            RemoveSpecialCommentsClean(ref line);
            line = line?.Trim();
            if (LineIsValid(line, out string normalizedLine))
            {
                string[] commands = [normalizedLine];
                int pileIndex = 1;
                Match match = Regex.Match(normalizedLine, @"^p(-?\d*):");
                if (match.Success)
                {
                    commands = SplitPile(normalizedLine); // Parses the pile of commands

                    string numberStr = match.Groups[1].Value;
                    if (!string.IsNullOrEmpty(numberStr))
                    {
                        if (int.TryParse(numberStr, out int newPileIndex))
                        {
                            ValidateIntegerLength(ref newPileIndex, 1, int.MaxValue, "Invalid value encountered", "Pile index out of range",
                                "Index must be greater than 0. Defaulting to 1.", $"Index exceeds maximum value ({int.MaxValue}). Defaulting to {int.MaxValue}.");
                            pileIndex = Math.Clamp(newPileIndex, 1, int.MaxValue);
                        }
                    }
                }
                for (int i = 0; i < pileIndex; i++)
                {
                    CommandStringArgumentReplaceBuffer.PileIndex = i;
                    for (int j = 0; j < commands.Length; j++)
                    {
                        ProcessCommand(commands[j], (block) =>
                        {
                            for (int k = j + 1; k < commands.Length; k++)
                            {
                                block.AdoptParsingBeforeExecution(commands[k]);
                            }
                            commands = Array.Empty<string>();
                        });
                    }
                }
            }
        }
        private static void ResetStringArgsBuffer()
        {
            CommandStringArgumentReplaceBuffer.PileIndex = 0;
        }
        public static bool ValidateIntegerLength(ref int value, int min, int max, string titleWarn, string nameWarn, string reasonLess, string reasonBigger, bool warn = true)
        {
            bool logged = false;
            if (value < min)
            {
                ConsoleEx.WriteError(titleWarn, nameWarn, reasonLess, warn);
                value = min;
                logged = true;
            }
            if (value > max)
            {
                ConsoleEx.WriteError(titleWarn, nameWarn, reasonBigger, warn);
                value = max;
                logged = true;
            }
            if (!warn && logged)
            {
                return false; // Prevents execution
            }
            return true;
        }
        public static void ProcessCommand(string command, Action<CommandBlockBase>? moveRestPileFunctionsOnCmdBlock = null)
        {
            string[] splits = SplitLine(command);
            if (splits.Length == 0)
            {
                return;
            }
            bool commandOnSecondAttempt = false;
            CommandBase? possibleCommand = CommandBase.GetCommandByPath(RootCommand == null ? splits[0] : $"{RootCommand.Path}/{splits[0]}/");
            if (possibleCommand == null)
            {
                possibleCommand = CommandBase.GetCommandByPath(splits[0]);
                commandOnSecondAttempt = possibleCommand != null;
            }
            if (commandOnSecondAttempt)
            {
                if (RootCommand != null && !possibleCommand!.IsGlobal)
                {
                    possibleCommand = null;
                }
            }

            if (possibleCommand != null && possibleCommand.IsWorthParsing())
            {
                Console.WriteLine();
                (Type, object?)[]? parsedArguments = null;
                if (splits.Length > 1)
                {
                    parsedArguments = ParseArguments(splits[1..], possibleCommand);
                }
                if (possibleCommand is CommandBlockBase block)
                {
                    moveRestPileFunctionsOnCmdBlock?.Invoke(block);
                }
                possibleCommand.Execute(parsedArguments, out bool fail);
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine();
                if (RootCommand != null)
                {
                    CommandBlockBase? commandBlock = (CommandBlockBase)RootCommand;
                    Console.WriteLine($"Command \"{RootCommand.Path}/{splits[0]}\" does not exist! Exit the block {(commandBlock == null ? "" : $"(\"{commandBlock.ExitCommandName}\")")} and use \"help\" command to see all available commands!");
                }
                else
                {
                    Console.WriteLine($"Command \"{splits[0]}\" does not exist! Use \"help\" command to see all available commands!");
                }
                Console.WriteLine();
            }
        }
        private static string[] SplitLine(string input)
        {
            var matches = Regex.Matches(input, @"(""[^""]*""|\S)+");
            var result = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                result[i] = matches[i].Value.Trim('"');
            }
            return result;
        }
        public static string[] SplitPile(string pile)
        {
            var prefixMatch = Regex.Match(pile, @"^p(-?\d*):");
            if (prefixMatch.Success)
            {
                pile = pile[prefixMatch.Length..];
            }

            string pattern = @"
        (?:^|;)                    # Start or semicolon
        \s*                        # Optional whitespace
        (                          # Capture group for command:
            (?:                     
                [^;""']+           # Non-semicolon, non-quote chars
                |""(?:\\""|[^""])*""  # OR double-quoted string with escapes
                |'(?:\\'|[^'])*'   # OR single-quoted string with escapes
            )+                     # (one or more)
        )
        \s*                       # Optional whitespace
        (?=$|;)                   # Until end or next semicolon
    ";

            var matches = Regex.Matches(pile, pattern,
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

            return matches
                .Cast<Match>()
                .Select(m => m.Groups[1].Value.Trim())
                .ToArray();
        }
        public static void RemoveSpecialCommentsClean(ref string? input)
        {
            const string pattern = @"
        (                   # Capture group 1: Either:
            (?:             # 
                ""[^""]*""  # Double-quoted string
                |'[^']*'    # Or single-quoted string
            )               #
        )                   # End capture group 1
        |                   # OR
        (                   # Capture group 2:
            \s*~            # Optional whitespace + ~
            [^~]*           # Comment content
            ~\s*           # ~ + optional whitespace
        )                   # End capture group 2
    ";

            input = Regex.Replace(input ?? string.Empty, pattern, m =>
                m.Groups[1].Success ? m.Groups[1].Value : " ",
                RegexOptions.IgnorePatternWhitespace)
                .Replace("  ", " ");
        }
        public static (Type Type, object? Value)[] ParseArguments(string[] inputs, CommandBase command)
        {
            var results = new (Type Type, object? Value)[inputs.Length];

            for (int i = 0; i < inputs.Length; i++)
            {
                results[i] = ParseArgument(inputs[i], command);
            }

            return results;
        }
        private static (Type Type, object? Value) ParseArgument(string input, CommandBase command)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (typeof(string), input);

            if (input.StartsWith("&"))
            {
                if (CommandStringArgumentReplaceBuffer.GetCustomVariableFullInfo(input, out (Type? type, object? value) info))
                {
                    if (info.type != default || info.value != default)
                    {
                        return (info.type!, info.value);
                    }
                }
            }

            // 1. Check for `null`
            if (input.Equals("null", StringComparison.OrdinalIgnoreCase))
                return (typeof(object), null);

            // 2. Check for boolean
            if (bool.TryParse(input, out bool boolValue))
                return (typeof(bool), boolValue);

            // 3. Check for integer (only Int32)
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                return (typeof(int), intValue);

            // 4. Check for floating-point (only float)
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                return (typeof(float), floatValue);

            // 5. Check for char (single character)
            if (input.StartsWith("\'") && input.EndsWith("\'") && input.Length == 3)
                return (typeof(char), input[1]);

            // 6. Check for DateTime (ISO 8601 format)
            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime dateTimeValue))
                return (typeof(DateTime), dateTimeValue);

            // 7. Check for Guid (UUID format)
            if (Guid.TryParse(input, out Guid guidValue))
                return (typeof(Guid), guidValue);

            // 8. Flags (--prefix)
            if (input.StartsWith("--"))
                return (typeof(CommandFlag), command.GetFlagByName(input[2..]));

            // 9. Strings ("Hello, world!")
            CommandStringArgumentReplaceBuffer.Parse(ref input, false);
            bool stringIsSpecialPrefixed = input.StartsWith("$");
            bool stringIsSpecialPrefixedComma = input.StartsWith("$\"");
            if (stringIsSpecialPrefixed || stringIsSpecialPrefixedComma)
            {
                string strArg = string.Empty;
                if (input.Length >= 2)
                {
                    strArg = input[(stringIsSpecialPrefixedComma ? 2 : 1)..];
                    CommandStringArgumentReplaceBuffer.Parse(ref strArg, true);
                }
                return (typeof(string), strArg);
            }
            return (typeof(string), input);
        }
        public static bool LineIsValid(string? line, out string nonNullLine)
        {
            if (!(line == null || line.Length <= 0 || line.Replace(" ", "") == string.Empty))
            {
                nonNullLine = line;
                return true;
            }
            else
            {
                nonNullLine = string.Empty;
                return false;
            }
        }
    }
}
