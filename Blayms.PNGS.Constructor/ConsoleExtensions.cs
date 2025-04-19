namespace Blayms.PNGS.Constructor
{
    internal static class ConsoleEx
    {
        private static Stack<ConsoleColor> consoleColorStack = new Stack<ConsoleColor>();
        private static string[] IndentCache = new string[11];
        private const string SingleIndent = "    ";
        private static int m_IndentLevel = 0;
        public static int IndentLevel
        {
            get
            {
                return m_IndentLevel;
            }
            set
            {
                m_IndentLevel = Math.Clamp(value, 0, 10);
            }
        }
        static ConsoleEx()
        {
            for (int i = 0; i < IndentCache.Length; i++)
            {
                IndentCache[i] = string.Concat(Enumerable.Repeat(SingleIndent, i));
            }
            IndentCache[0] = string.Empty;
        }
        public static string BuildIndent()
        {
            return IndentCache[m_IndentLevel];
        }
        public static void WriteLine()
        {
            Console.WriteLine();
        }
        public static void WriteLineFmt(string message, bool allowVariables = false, bool bypassIndent = false)
        {
            CommandStringArgumentReplaceBuffer.Parse(ref message, allowVariables);
            WriteLine(message, ConsoleColor.White, bypassIndent);
        }
        public static void WriteLine(object msg, ConsoleColor consoleColor = ConsoleColor.White, bool bypassIndent = false)
        {
            bool sameColorNow = Console.ForegroundColor == consoleColor;
            if (!sameColorNow)
            {
                BeginForegroundColor(consoleColor);
            }
            Console.WriteLine($"{(bypassIndent ? "" : BuildIndent())}{msg}");
            if (!sameColorNow)
            {
                EndForegroundColor();
            }
        }
        public static void WriteSignature()
        {
            WriteLine(@"
░▒▓███████▓▒░  ░▒▓███████▓▒░   ░▒▓██████▓▒░   ░▒▓███████▓▒░ 
░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░        
░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░        ░▒▓█▓▒░        
░▒▓███████▓▒░  ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒▒▓███▓▒░  ░▒▓██████▓▒░  
░▒▓█▓▒░        ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░        ░▒▓█▓▒░ 
░▒▓█▓▒░        ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░        ░▒▓█▓▒░ 
░▒▓█▓▒░        ░▒▓█▓▒░░▒▓█▓▒░  ░▒▓██████▓▒░  ░▒▓███████▓▒░  
                                                            
                                                            
 ░▒▓██████▓▒░  ░▒▓████████▓▒░  ░▒▓██████▓▒░  ░▒▓███████▓▒░  
░▒▓█▓▒░░▒▓█▓▒░    ░▒▓█▓▒░     ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓█▓▒░           ░▒▓█▓▒░     ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓█▓▒░           ░▒▓█▓▒░     ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓███████▓▒░  
░▒▓█▓▒░           ░▒▓█▓▒░     ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ 
░▒▓█▓▒░░▒▓█▓▒░    ░▒▓█▓▒░     ░▒▓█▓▒░░▒▓█▓▒░ ░▒▓█▓▒░░▒▓█▓▒░ 
 ░▒▓██████▓▒░     ░▒▓█▓▒░      ░▒▓██████▓▒░  ░▒▓█▓▒░░▒▓█▓▒░ 
                                                            
                    Made by Blayms
                     Version 1.0
             Type ""help"" for commands
", GenerateConsoleColor());
            Console.WriteLine();
        }
        public static void Write(object msg, ConsoleColor consoleColor = ConsoleColor.White)
        {
            bool sameColorNow = Console.ForegroundColor == consoleColor;
            if (!sameColorNow)
            {
                BeginForegroundColor(consoleColor);
            }
            Console.Write($"{BuildIndent()}{msg}");
            if (!sameColorNow)
            {
                EndForegroundColor();
            }
        }
        public static int Read(ConsoleColor consoleColor = ConsoleColor.White)
        {
            bool sameColorNow = Console.ForegroundColor == consoleColor;
            if (!sameColorNow)
            {
                BeginForegroundColor(consoleColor);
            }
            int character = Console.Read();
            if (!sameColorNow)
            {
                EndForegroundColor();
            }
            return character;
        }
        public static void WriteError(string title, string name, string reason, bool warning = false)
        {
           WriteLine($"{title}:\n{name}: {reason}\n", warning? ConsoleColor.Yellow : ConsoleColor.Red);
        }
        public static string? ReadLine(ConsoleColor consoleColor = ConsoleColor.White)
        {
            bool sameColorNow = Console.ForegroundColor == consoleColor;
            if (!sameColorNow)
            {
                BeginForegroundColor(consoleColor);
            }
            string? line = Console.ReadLine()?.Trim();
            if (!sameColorNow)
            {
                EndForegroundColor();
            }
            return line;
        }
        public static ConsoleKeyInfo ReadKey(ConsoleColor consoleColor = ConsoleColor.White)
        {
            bool sameColorNow = Console.ForegroundColor == consoleColor;
            if (!sameColorNow)
            {
                BeginForegroundColor(consoleColor);
            }
            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            if (!sameColorNow)
            {
                EndForegroundColor();
            }
            return consoleKeyInfo;
        }
        public static ConsoleColor GenerateConsoleColor()
        {
            return (ConsoleColor)CommandParser.Random.Next(0, 15);
        }
        public static void BeginForegroundColor(ConsoleColor consoleColor)
        {
            consoleColorStack.Push(Console.ForegroundColor);
            Console.ForegroundColor = consoleColor;
        }
        public static void EndForegroundColor()
        {
            if (consoleColorStack.Count > 0)
            {
                Console.ForegroundColor = consoleColorStack.Pop();
            }
        }
    }
}
