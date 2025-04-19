namespace Blayms.PNGS.Constructor.Commands
{
    public class HelpCommand : CommandBase
    {
        public override string Name => "help";
        public override string Description => "Describes all commands available in PNGS Constructor!";
        public override bool IsGlobal => true;
        protected override void OnRegistered()
        {
            Flags = 
            [
                new CommandFlag("fullpath")
            ];
        }
        public override void Execute((Type, object?)[]? args, out bool fail)
        {
            base.Execute(args, out fail);

            fail = false;
            ConsoleEx.IndentLevel++;
            AsciiTableBuilder asciiTable = new AsciiTableBuilder
            {
                Alignment = TableAlignment.Middle,
                MaxCellWidth = 60,
                HorizontalBorder = "=",
                RowSeparator = "=",
                VerticalBorder = "||",
                CornerBorder = "++",
                CellPadding = 2
            };
            asciiTable.SetHeaders("Command", "Description");
            IEnumerator<CommandBase> enumerator = GetAllCommands();
            if(CommandParser.RootCommand != null)
            {
                enumerator = GetAllSubCommandsOf(CommandParser.RootCommand);
            }

            PrintHelpSignature();
            ConsoleEx.WriteLine("Here is the list of all useful shortcuts:");
            Console.WriteLine();
            ConsoleEx.WriteLine("Command Pile Mode", ConsoleColor.DarkGreen);
            ConsoleEx.IndentLevel++;
            ConsoleEx.WriteLineFmt("Use the \"p[#red][#it]NUMBER[it#][clr#]:\" prefix at the start of a line to execute multiple commands sequentially.");
            ConsoleEx.WriteLine("Commands should be separated by semicolons (;) in a single line.");
            ConsoleEx.WriteLineFmt("By the way, the [#red][#it]NUMBER[it#][clr#] here is optional. It will default to 1 if omitted.");
            ConsoleEx.WriteLine("Example: p: print \"Hello, World!\"; info; info --github; print \"Pile!\"");
            ConsoleEx.IndentLevel--;
            Console.WriteLine();
            ConsoleEx.WriteLine("Custom variable declaration", ConsoleColor.DarkGreen);
            ConsoleEx.IndentLevel++;
            ConsoleEx.WriteLine("You can declare, modify, reference, and delete custom variables!");
            ConsoleEx.WriteLineFmt("All of this is done using the \"[#blink]declvar[blink#]\" command. Note: To delete a variable, pass \"null\" as the value!");
            ConsoleEx.WriteLine("These declared variables can later be reused (with & symbol at the start). This is a very useful");
            ConsoleEx.WriteLine("way to provide paths when executing a file that may require a specific path!");
            ConsoleEx.WriteLineFmt("Example: [#dim]print \"&customVarName\"");
            ConsoleEx.IndentLevel--;
            Console.WriteLine();
            ConsoleEx.WriteLine("File Execution", ConsoleColor.DarkGreen);
            ConsoleEx.IndentLevel++;
            ConsoleEx.WriteLine("Not only can you run multiple commands using the pile feature, but you can also execute scripts from a file!");
            ConsoleEx.WriteLineFmt("This is done using the \"[#blink]exec[blink#]\" command followed by the file path.");
            ConsoleEx.WriteLine("Example script:");
            ConsoleEx.WriteLine("~ This program runs for 60 seconds ~", ConsoleColor.DarkGray);
            ConsoleEx.WriteLine("p60: ~ Initialize a pile ~", ConsoleColor.DarkGray);
            ConsoleEx.WriteLine("print $\"$pnum! Declared variable $game\"; ~ Print current pile index ~", ConsoleColor.DarkGray);
            ConsoleEx.WriteLine("sleep 1000; ~ Sleep for 1 second ~", ConsoleColor.DarkGray);
            ConsoleEx.IndentLevel--;
            Console.WriteLine();
            ConsoleEx.WriteLine("Comments", ConsoleColor.DarkGreen);
            ConsoleEx.IndentLevel++;
            ConsoleEx.WriteLineFmt("As shown in the example above, comments should be formatted like this: [#blink]~ COMMENT HERE ~[blink#]");
            ConsoleEx.IndentLevel--;
            Console.WriteLine();
            ConsoleEx.WriteLine("Formatting & Predefined Variables", ConsoleColor.DarkGreen);
            ConsoleEx.WriteLineFmt("You can manipulate strings in interesting ways. See all examples in [#uln][#blink]https://github.com/blayms/Portable-Network-Graphics-Sequence-File[blink#][uln#]");
            ConsoleEx.IndentLevel++;
            Console.WriteLine();
            ConsoleEx.WriteLine("Formatting", ConsoleColor.DarkGreen);
            ConsoleEx.WriteLine("You can format strings using tags that look like \"[#TAG]\". Some tags need to be closed with \"[TAG#]\" as well.");
            ConsoleEx.WriteLine("Examples: \"Hello,[#br]World!\" creates a line break, \"[#blink]Blink[blink#]ing\" makes text blink, \"[#cyan]Cyan Text!clr#\" changes color");
            ConsoleEx.WriteLineFmt($"\"Hello,[#br]{ConsoleEx.BuildIndent()}World!\", \"[#blink]Blink[blink#]ing\", \"[#cyan]Cyan Text![clr#]\"");
            Console.WriteLine();
            ConsoleEx.WriteLine("Predefined Variables", ConsoleColor.DarkGreen);
            ConsoleEx.WriteLine("You can insert variables in strings using the \"$\" symbol followed by a variable name");  // Improved clarity
            ConsoleEx.WriteLine("Examples: $pc (Computer Name), $pnum (Current Pile Iteration Index), $date (Filename-friendly date), $desktop (Desktop path)");
            ConsoleEx.WriteLineFmt("$pc, $pnum, $date, $desktop", true);
            ConsoleEx.IndentLevel--;
            Console.WriteLine();
            ConsoleEx.WriteLine("Here is the list of all commands:");
            Console.WriteLine();
            ConsoleEx.IndentLevel++;
            ConsoleEx.WriteLine("Arguments are enclosed in \"<>\", and flags are specified after \":\"", ConsoleColor.DarkGreen);
            ConsoleEx.WriteLine("A \"?\" next to a parameter indicates that it's optional and has a default value", ConsoleColor.DarkGreen);
            Console.WriteLine();
            ConsoleEx.WriteLine("If you're having trouble viewing this table properly, try:", ConsoleColor.DarkCyan);
            ConsoleEx.WriteLine("*   Resizing the console window", ConsoleColor.DarkCyan);
            ConsoleEx.WriteLine("*   Adjusting zoom level for better readability", ConsoleColor.DarkCyan);
            ConsoleEx.IndentLevel--;
            Console.WriteLine();
            while (enumerator.MoveNext())
            {
                CommandBase commandBase = enumerator.Current;
                if (!(commandBase.IsWorthParsing()) || (CommandParser.RootCommand == null && commandBase.Owner != null) ||
        (CommandParser.RootCommand != null && commandBase.Owner != CommandParser.RootCommand))
                {
                    continue;
                }
                asciiTable.AddRow(((GetFlagByIndex(0)?.IsRaised ?? false) ? commandBase.ToString() : commandBase.ToStringWName() ), commandBase.Description);
            }
            ConsoleEx.WriteLineFmt(asciiTable.ToString(), false, true);
            ConsoleEx.IndentLevel--;
            Reset();
        }
        private static void PrintHelpSignature()
        {
            ConsoleEx.WriteLine(@"
░▒▓█▓▒░░▒▓█▓▒░▒▓████████▓▒░▒▓█▓▒░      ░▒▓███████▓▒░  
░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░      ░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░      ░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓████████▓▒░▒▓██████▓▒░ ░▒▓█▓▒░      ░▒▓███████▓▒░  
░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░      ░▒▓█▓▒░      ░▒▓█▓▒░        
░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░      ░▒▓█▓▒░      ░▒▓█▓▒░        
░▒▓█▓▒░░▒▓█▓▒░▒▓████████▓▒░▒▓████████▓▒░▒▓█▓▒░        
", ConsoleEx.GenerateConsoleColor());
            Console.WriteLine();
        }
    }
}
