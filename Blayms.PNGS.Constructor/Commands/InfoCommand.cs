namespace Blayms.PNGS.Constructor.Commands
{
    internal class InfoCommand : CommandBase
    {
        public override string Name => "info";
        public override string Description => "Prints information about this tool!";
        public override bool IsGlobal => true;

        protected override void OnRegistered()
        {
            Flags =
            [
                new CommandFlag("github"),
            ];
        }

        public override void Execute((Type, object?)[]? args, out bool fail)
        {
            base.Execute(args, out fail);

            if (!fail)
            {
                CommandFlag? gitFlag = GetFlagByIndex(0);
                if (!(gitFlag == null ? false : gitFlag.IsRaised))
                {
                    ConsoleEx.WriteLine($"{AppInformation.FullName}");
                    ConsoleEx.IndentLevel++;
                    ConsoleEx.WriteLine($"{AppInformation.Description}");
                    ConsoleEx.WriteLine($"Author: {AppInformation.Author}");
                    ConsoleEx.WriteLine($"Version: {AppInformation.Version}");
                    ConsoleEx.IndentLevel--;
                    Console.WriteLine();
                    ConsoleEx.WriteLine($"Type \"{Name} {CommandFlag.Prefix}{Flags![0].Name}\" to get a github repo link!");
                }
                else
                {
                    Console.WriteLine(AppInformation.GithubURL);
                }
            }

            Reset();
        }
    }
}
