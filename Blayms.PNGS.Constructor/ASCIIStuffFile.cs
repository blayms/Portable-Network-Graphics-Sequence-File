using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Blayms.PNGS.Constructor
{
    internal static class ASCIIStuffFile
    {
        private static readonly Dictionary<string, ASCIIArt> asciiArtDict = new();
        public static int Count => asciiArtDict.Count;

        public static void ReadFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("Blayms.PNGS.Constructor.Resources.asciistuff.txt");
            if (stream == null)
                throw new FileNotFoundException("Embedded resource not found.");

            string fileContent = TryReadWithEncodings(stream, Encoding.UTF8);
            var matches = Regex.Matches(fileContent, @"ascii\((.*?)\)\s*starts\s*(.*?)\s*ends", RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                string name = match.Groups[1].Value.Trim();
                string art = match.Groups[2].Value.Trim();

                if (!asciiArtDict.ContainsKey(name))
                {
                    asciiArtDict[name] = new ASCIIArt(name, art);
                }
            }
        }

        public static ASCIIArt GetArtByName(string name)
        {
            return asciiArtDict.TryGetValue(name, out var art) ? art : null!;
        }
        public static bool ContainsArt(string name)
        {
            if(name == null)
            {
                return false;
            }
            return asciiArtDict.ContainsKey(name);
        }
        public static IReadOnlyCollection<ASCIIArt> GetAllArt()
        {
            return asciiArtDict.Values;
        }

        private static string TryReadWithEncodings(Stream input, params Encoding[] encodings)
        {
            foreach (var encoding in encodings)
            {
                input.Position = 0;
                using var reader = new StreamReader(input, encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                string content = reader.ReadToEnd();

                if (IsAsciiArtContentValid(content))
                    return content;
            }

            throw new InvalidDataException("Unable to read ASCII art content with available encodings.");
        }

        private static bool IsAsciiArtContentValid(string content)
        {
            return !content.Contains('�');
        }
    }

}
