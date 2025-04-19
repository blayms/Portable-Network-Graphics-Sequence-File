namespace Blayms.PNGS.Constructor.Commands
{
    public class DeclareCustomVariableCommand : CommandBase
    {
        public override string Name => "declvar";
        public override string Description => "Declare, set or delete a custom string variable";
        public override bool IsGlobal => true;

        protected override void OnRegistered()
        {
            ArgumentInfo = new
            (
                ("name", (typeof(string), false, string.Empty)),
                ("value", (typeof(object), false, null!))
            );
        }

        public override void Execute((Type, object?)[]? args, out bool fail)
        {
            base.Execute(args, out fail);

            string name = ExpectArgumentInstance<string>(ref args, 0, ref fail);
            object value = ExpectArgumentInstance<object>(ref args, 1, ref fail);
            if (!fail)
            {
                if (value != null)
                {
                    if (CommandStringArgumentReplaceBuffer.DeclareCustomVariable(name, value))
                    {
                        ConsoleEx.WriteLine($"Declared: {value.GetType().Name} {name} = {value}");
                    }
                    else
                    {
                        ConsoleEx.WriteError("Unable to declare a variable", "Invalid variable name", "Name should not contain symbols like: \"$\", \"&\", \"#\", \"[\", \"]\"!");
                    }
                }
                else
                {
                    if (CommandStringArgumentReplaceBuffer.RemoveCustomVariable(name))
                    {
                        ConsoleEx.WriteLine($"Removed {name} variable!");
                    }
                    else
                    {
                        ConsoleEx.WriteError("Unable to delete a variable", "Invalid variable", $"Variable {name} does not exist!");
                    }
                }
            }

            Reset();
        }
        protected override void Reset()
        {

        }
    }
}
