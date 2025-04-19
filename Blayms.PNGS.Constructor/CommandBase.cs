namespace Blayms.PNGS.Constructor
{
    public class CommandBase
    {
        public virtual string Name => "";
        public virtual string Description => "";
        public string Path => Owner != null ? $"{Owner.Path}/{Name}/" : Name;
        public virtual CommandType Type => CommandType.Once;
        public virtual CommandHideFlags HideFlags => CommandHideFlags.None;
        public virtual bool IsGlobal => false; 
        public CommandBase? Owner { get; private set; } = null;
        public CommandBase[]? SubjugatedCommands { get; protected set; } = null;
        public CommandFlag[]? Flags { get; protected set; } = null;
        public CommandArgsInfo? ArgumentInfo { get; protected set; } = null;
        public CommandBase()
        {

        }
        public CommandBase(CommandBase? owner)
        {
            Owner = owner;
            RegisterInstance(this);
        }
        public CommandFlag? GetFlagByName(string name)
        {
            return Flags?.Where(x => x.Name ==  name).FirstOrDefault();
        }
        public CommandFlag GetFlagByIndex(int index)
        {
            return Flags?[index]!;
        }
        public string ToStringWName()
        {
            var argInfo = ArgumentInfo?.ToString() ?? "";
            string flagsPart = "";

            if (Flags is { Length: > 0 })
            {
                for (int i = 0; i < Flags.Length; i++)
                {
                    flagsPart += $":{CommandFlag.Prefix}{Flags[i].Name}";
                }
            }

            return $"{Name}{argInfo}{flagsPart}";
        }
        public override string ToString()
        {
            var argInfo = ArgumentInfo?.ToString() ?? "";
            string flagsPart = "";

            if (Flags is { Length: > 0 })
            {
                for (int i = 0; i < Flags.Length; i++)
                {
                    flagsPart += $":{CommandFlag.Prefix}{Flags[i].Name}";
                }
            }

            return $"{Path}{argInfo}{flagsPart}";
        }
        public bool IsWorthParsing()
        {
            switch (HideFlags)
            {
                case CommandHideFlags.None:
                case CommandHideFlags.HideFromHelp:
                    return true;
                case CommandHideFlags.DebugOnly:
#if DEBUG
                    return true;
#else
                    return false;
#endif
                default:
                    return false;
            }
        }
        protected virtual void OnRegistered()
        {

        }
        public void SubjugateExecutionTo(CommandBase owner)
        {
            if (!IsGlobal)
            {
                Owner = owner;
            }
            else
            {
                throw new Exception("Cannot subjugate a global command!");
            }
        }
        public virtual void Execute((Type, object?)[]? args, out bool fail)
        {
            fail = false;
            CommandFlag[] flags = PullFlagsFromArgs(ref args);
            ValidateArgumentCount(args);

            for (int i = 0; i < flags.Length; i++)
            {
                ref CommandFlag flag = ref flags[i];
                flag.Raise(true); // Raises all flags found in a command
            }

            if (Owner != null)
            {
                Owner.SubjugatedExecution(this);
            }
        }
        private static CommandFlag[] PullFlagsFromArgs(ref (Type, object?)[]? args)
        {
            if (args == null || args.Length == 0)
            {
                args = Array.Empty<(Type, object?)>();
                return Array.Empty<CommandFlag>();
            }

            var original = args;
            var flags = new List<CommandFlag>();
            var newArgs = new (Type, object?)[original.Length];
            int newCount = 0;

            for (int i = 0; i < original.Length; i++)
            {
                var (type, value) = original[i];
                if (type == typeof(CommandFlag) && value is CommandFlag flag)
                {
                    flags.Add(flag);
                }
                else
                {
                    newArgs[newCount++] = original[i];
                }
            }

            if (newCount != original.Length)
            {
                Array.Resize(ref newArgs, newCount);
                args = newArgs;
            }
            return flags.Count == 0 ? Array.Empty<CommandFlag>() : [.. flags];
        }

        protected virtual void Reset()
        {
            for (int i = 0; i < Flags?.Length; i++)
            {
                if (Flags[i].IsRaised)
                {
                    Flags[i].Raise(false); // On reset we lower all flags that were raised
                }
            }
        }
        private void ValidateArgumentCount((Type, object?)[]? args)
        {
            if (ArgumentInfo == null)
            {
                if (args != null && args.Length > 0)
                {
                    ConsoleEx.WriteError("Argument count validation failed",
                        "Argument count misconception",
                        $"Expected 0 arguments, but got {args.Length} instead!\n\nat {this}");
                }
                return;
            }

            int providedCount = args?.Length ?? 0;

            if (providedCount < ArgumentInfo.NecessarilyCount)
            {
                ConsoleEx.WriteError("Argument count validation failed",
                    "Missing required arguments",
                    $"Expected at least {ArgumentInfo.NecessarilyCount} required argument(s), but got {providedCount}!\n\nat {this}");
                return;
            }
            if (providedCount > ArgumentInfo.Count)
            {
                ConsoleEx.WriteError("Argument count validation failed",
                    "Too many arguments",
                    $"Expected at most {ArgumentInfo.Count} argument(s), but got {providedCount}!\n\nat {this}");
            }
        }
        public virtual void SubjugatedExecution(CommandBase command)
        {

        }

        private static List<CommandBase> commands = new();
        public static void Register(Type commandType)
        {
            if (commandType == null)
            {
                return;
            }
            if (commandType.Namespace != null && commandType.Namespace.EndsWith("Constructor.Commands"))
            {
                CommandBase? command = Activator.CreateInstance(commandType) as CommandBase;
                RegisterInstance(command);
            }
        }
        public static void RegisterInstance(CommandBase? instance)
        {
            if (instance == null)
            {
                return;
            }
            if (instance != null)
            {
                commands.Add(instance);
            }
            instance?.OnRegistered();
        }
        public static IEnumerator<CommandBase> GetAllCommands()
        {
            return commands.GetEnumerator();
        }
        public static IEnumerator<CommandBase> GetAllSubCommandsOf(CommandBase command)
        {
            return commands.Where(x => x.Owner == command).GetEnumerator();
        }
        private bool jumpOnceNextTimeWhenExceptingArgumentInstance;
        public T ExpectArgumentInstance<T>(ref (Type, object?)[]? args, int index, ref bool fail)
        {
            bool jumpDone = false;
            if (jumpOnceNextTimeWhenExceptingArgumentInstance)
            {
                index = Math.Clamp(index, 0, ArgumentInfo == null ? 0 : ArgumentInfo.Count);
                jumpDone = true;
            }
            if (fail)
            {
                return default!;
            }
            string? nameToExpect = ArgumentInfo?.GetNameByIndex(index);
            Type? typeToExpect = ArgumentInfo?.GetTypeByIndex(index);
            bool hasDefaultValue = ArgumentInfo == null ? false : ArgumentInfo.GetHasDefaultValueByIndex(index);
            object? defaultValue = ArgumentInfo?.GetDefaultValueByIndex(index);
            if (((args?.Length == 0 || args == null) || (args?.Length <= index) || (args?[index].Item1 != typeToExpect)) && hasDefaultValue)
            {
                jumpOnceNextTimeWhenExceptingArgumentInstance = true;
                fail = false;
                return (T)defaultValue!;
            }
            try
            {
                if (args?[index].Item1 != typeToExpect && typeof(T) != typeof(object))
                {
                    string receivedInstead = $"{args?[index].Item1.Name}{(args?[index].Item1.Name == nameof(CommandFlag) ? "" : $": {args?[index].Item2}")}";
                    ConsoleEx.WriteError("Command execution terminated", "Argument mismatch", $"Expected <{typeToExpect?.Name}: {nameToExpect}> at argument #{index}, but received <{((receivedInstead == ": ") ? "undefined" : receivedInstead)}> instead!\nat {this}");
                    fail = true;
                    return default!;
                }

            if (jumpDone)
            {
                jumpOnceNextTimeWhenExceptingArgumentInstance = false;
            }

            return (T)args?[index].Item2!;
            }
            catch
            {
                fail = true;
                return default!;
            }
        }
        public static CommandBase? GetCommandByPath(string? path)
        {
            if (path == null)
            {
                return null;
            }
            else
            {
                return commands.Where(x => x.Path == path).FirstOrDefault();
            }
        }
    }
}
