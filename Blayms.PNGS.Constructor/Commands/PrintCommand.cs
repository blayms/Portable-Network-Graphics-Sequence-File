namespace Blayms.PNGS.Constructor.Commands
{
    internal class PrintCommand : CommandBase
    {
        public override string Name => "print";
        public override string Description => "Prints a string value to console!";
        public override bool IsGlobal => true;

        private bool lower;
        private bool upper;

        protected override void OnRegistered()
        {
            ArgumentInfo = new
            (
                ("message", (typeof(object), false, string.Empty))
            );
            Flags =
            [
                new CommandFlag("lower", () =>
                {
                    lower = true;
                    upper = false;
                }),
                new CommandFlag("upper", () =>
                {
                    lower = false;
                    upper = true;
                })
            ];
        }

        public override void Execute((Type, object?)[]? args, out bool fail)
        {
            base.Execute(args, out fail);

            object message = ExpectArgumentInstance<object>(ref args, 0, ref fail);
            if (!fail)
            {
                string? strMsg = message.ToString();
                if (lower)
                {
                    strMsg = strMsg?.ToLower();
                }
                if (upper)
                {
                    strMsg = strMsg?.ToUpper();
                }
                Console.WriteLine(strMsg);
            }
            Reset();
        }
    }
}
