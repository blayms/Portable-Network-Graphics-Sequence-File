namespace Blayms.PNGS.Constructor
{
    public class CommandBlockBase : CommandBase
    {
        public virtual string ExitCommandName => "exit";
        public override CommandType Type => CommandType.ModeEnter;
        private List<string> adoptedCmdsToParse = new();
        /// <summary>
        /// Use this to clean data that was maybe stored by a command block instance
        /// </summary>
        public virtual void OnExit(ref bool keepBlockRunning)
        {
            CommandParser.RootCommand = Owner;
        }
        public void AdoptParsingBeforeExecution(string command)
        {
            adoptedCmdsToParse.Add(command);
        }
        public override void Execute((Type, object?)[]? args, out bool fail)
        {
            if (args == null)
            {
                args = Array.Empty<(Type, object?)>();
            }
            bool runBlock = true;
            CommandParser.RootCommand = this;

            for (int i = 0; i < adoptedCmdsToParse.Count; i++)
            {
                if (adoptedCmdsToParse[i] == ExitCommandName)
                {
                    Console.WriteLine();
                    OnExit(ref runBlock);
                }
                else
                {
                    CommandParser.ProcessCommand(adoptedCmdsToParse[i]);
                }
            }
            adoptedCmdsToParse.Clear();

            fail = false;
            while(runBlock)
            {
                string? line = Console.ReadLine()?.Trim();
                if (CommandParser.LineIsValid(line, out string normalizedLine))
                {
                    if (normalizedLine != ExitCommandName)
                    {
                        CommandParser.ProcessLine(line);
                    }
                    else
                    {
                        Console.WriteLine();
                        runBlock = false;
                        OnExit(ref runBlock);
                    }
                }
            }
        }
    }
}
