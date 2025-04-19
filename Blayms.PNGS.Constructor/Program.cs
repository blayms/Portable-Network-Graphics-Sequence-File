using Blayms.PNGS.Constructor;
using System.Reflection;

ConsoleEx.WriteSignature();

// Link commands

Assembly assembly = Assembly.GetExecutingAssembly();

IEnumerator<Type> commandTypes = assembly.GetTypes().Where(x => (x.Namespace != null
&& x.Namespace.EndsWith("Constructor.Commands"))
&& !(x.FullName ?? string.Empty).Contains("+")).GetEnumerator();

while (commandTypes.MoveNext())
{
    CommandBase.Register(commandTypes.Current);
}

// Load all ASCII arts from embedded asciistuff.txt file FOR FUN!
ASCIIStuffFile.ReadFile();

// Run command parser
CommandParser.Run(args);