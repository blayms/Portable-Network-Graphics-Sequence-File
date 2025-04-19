namespace Blayms.PNGS.Constructor.Commands
{
    public class SleepCommand : CommandBase
    {
        public override string Name => "sleep";
        public override string Description => "Pauses a main thread for specified amount of milliseconds";
        public override bool IsGlobal => true;

        protected override void OnRegistered()
        {
            ArgumentInfo = new
            (
                ("milliseconds", (typeof(int), false, 1))
            );
        }

        public override void Execute((Type, object?)[]? args, out bool fail)
        {
            base.Execute(args, out fail);

            int milliseconds = ExpectArgumentInstance<int>(ref args, 0, ref fail);

            if (!fail)
            {
                Thread.Sleep(milliseconds);
            }

            Reset();
        }
        protected override void Reset()
        {

        }
    }
}
