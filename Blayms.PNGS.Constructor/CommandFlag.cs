namespace Blayms.PNGS.Constructor
{
    public class CommandFlag
    {
        public const string Prefix = "--";
        public string Name;
        public bool IsRaised { get; private set; }
        private Action? onRaised, onLowered;
        public CommandFlag(string name, Action? onRaised = null, Action? onLowered = null)
        {
            Name = name;
            this.onRaised = onRaised;
            this.onLowered = onLowered;
        }
        public void Raise(bool value)
        {
            IsRaised = value;
            (IsRaised ? onRaised : onLowered)?.Invoke();
        }
    }
}
