namespace Blayms.PNGS.Constructor.Commands
{
    internal class ExecuteCodeCommand : CommandBase
    {
        public override string Name => "exec";
        public override string Description => "Executes code straight from file contents";
        public override bool IsGlobal => true;

        protected override void OnRegistered()
        {
            ArgumentInfo = new
            (
                ("path", (typeof(string), false, string.Empty))
            );
        }

        public override void Execute((Type, object?)[]? args, out bool fail)
        {
            base.Execute(args, out fail);

            string path = ExpectArgumentInstance<string>(ref args, 0, ref fail);

            if (!File.Exists(path))
            {
                ConsoleEx.WriteError("Code execution failed", "File Error", $"Does not exist at {path}");
                fail = true;
            }
            if (!fail)
            {
                CommandParser.ProcessLine(File.ReadAllText(path).Replace(Environment.NewLine, ""));
            }

            Reset();
        }
        protected override void Reset()
        {

        }
    }
}
