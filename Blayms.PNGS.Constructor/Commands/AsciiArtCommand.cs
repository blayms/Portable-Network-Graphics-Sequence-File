namespace Blayms.PNGS.Constructor.Commands
{
    internal class AsciiArtCommand : CommandBase
    {
        public override string Name => "asciiart";
        public override string Description => "Show some fun ascii art!";
        public override CommandHideFlags HideFlags => CommandHideFlags.HideFromHelp;

        protected override void OnRegistered()
        {
            ArgumentInfo = new
            (
                ("name", (typeof(string), false, string.Empty)),
                ("color", (typeof(string), true, "White"))
            );
        }

        public override void Execute((Type, object?)[]? args, out bool fail)
        {
            base.Execute(args, out fail);

            string name = ExpectArgumentInstance<string>(ref args, 0, ref fail);
            string color = ExpectArgumentInstance<string>(ref args, 1, ref fail);
            if (!ASCIIStuffFile.ContainsArt(name))
            {
                fail = true;
                ConsoleEx.WriteError("Error drawing ASCII art", "Unknown ASCII art name", $"Failed to find ASCII art titled \"{name ?? "UNDEFINED"}\"!");
            }
            ConsoleColor asciiColor = ConsoleColor.White;
            if (!fail && !Enum.TryParse(color, true, out asciiColor))
            {
                fail = true;
                ConsoleEx.WriteError("Error drawing ASCII art", "Unknown Console Color", $"Failed to parse ConsoleColor.{color ?? "UNDEFINED"}!");
            }

            if (!fail)
            {
                ConsoleEx.WriteLine
                ($@"

{ASCIIStuffFile.GetArtByName(name).Art}

                ", asciiColor);
            }

            Reset();
        }
        protected override void Reset()
        {

        }
    }
}
