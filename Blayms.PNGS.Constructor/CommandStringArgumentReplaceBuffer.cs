using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.CSharp.RuntimeBinder;

namespace Blayms.PNGS.Constructor
{
    internal static class CommandStringArgumentReplaceBuffer
    {
        internal static int PileIndex;
        internal static int LastSeqIndex;
        internal static int LastHeaderMetadataIndex;
        internal static int LastSeqLength;
        private static readonly Dictionary<string, Func<string?>> _replacementsForAll = new()
        {
            // ===== FORMATTING =====
            ["[#tab]"] = () => "\t",
            ["[#nbsp]"] = () => "\u00A0",
            ["[#br]"] = () => "\n",
            ["[#brn]"] = () => "\r\n",
            ["[#esc]"] = () => "\x1B",

            // ===== TEXT COLORS (ANSI) =====
            ["[#black]"] = () => "\x1b[30m",
            ["[#red]"] = () => "\x1b[31m",
            ["[#green]"] = () => "\x1b[32m",
            ["[#yellow]"] = () => "\x1b[33m",
            ["[#blue]"] = () => "\x1b[34m",
            ["[#magenta]"] = () => "\x1b[35m",
            ["[#cyan]"] = () => "\x1b[36m",
            ["[#white]"] = () => "\x1b[37m",

            // ===== BACKGROUND COLORS =====
            ["[#bgblack]"] = () => "\x1b[40m",
            ["[#bgred]"] = () => "\x1b[41m",
            ["[#bggreen]"] = () => "\x1b[42m",
            ["[#bgyellow]"] = () => "\x1b[43m",
            ["[#bgblue]"] = () => "\x1b[44m",
            ["[#bgmagenta]"] = () => "\x1b[45m",
            ["[#bgcyan]"] = () => "\x1b[46m",
            ["[#bgwhite]"] = () => "\x1b[47m",

            // ===== TEXT STYLES =====
            ["[#bold]"] = () => "\x1b[1m",
            ["[#dim]"] = () => "\x1b[2m",
            ["[#it]"] = () => "\x1b[3m",
            ["[#uln]"] = () => "\x1b[4m",
            ["[#blink]"] = () => "\x1b[5m",
            ["[#inv]"] = () => "\x1b[7m",
            ["[#hidden]"] = () => "\x1b[8m",
            ["[#strike]"] = () => "\x1b[9m",

            // ===== RESETS =====
            ["[fmt#]"] = () => "\x1b[0m",
            ["[clr#]"] = () => "\x1b[39m",
            ["[bg#]"] = () => "\x1b[49m",
            ["[bold#]"] = () => "\x1b[22m",
            ["[it#]"] = () => "\x1b[23m",
            ["[uln#]"] = () => "\x1b[24m",
            ["[blink#]"] = () => "\x1b[25m",
            ["[inv#]"] = () => "\x1b[27m",
            ["[hidden#]"] = () => "\x1b[28m",
            ["[strike#]"] = () => "\x1b[29m",

            // ===== SPECIAL UNICODE =====
            ["[#check]"] = () => "\u2713", 
            ["[#cross]"] = () => "\u2717",
            ["[#arrow]"] = () => "\u2192",
            ["[#bullet]"] = () => "\u2022",
            ["[#sigma]"] = () => "\u03A3",
            ["[#pi]"] = () => "\u03C0",
            ["[#omega]"] = () => "\u03A9",
            ["[#degree]"] = () => "\u00B0",
            ["[#copy]"] = () => "\u00A9",
    
            // ===== BOX DRAWING =====
            ["[#boxtl]"] = () => "\u250C",
            ["[#boxtr]"] = () => "\u2510",
            ["[#boxbl]"] = () => "\u2514",
            ["[#boxbr]"] = () => "\u2518",
            ["[#boxh]"] = () => "\u2500",
            ["[#boxv]"] = () => "\u2502",
        };
        private static readonly Dictionary<string, Func<object?>> _customDeclarations = new()
        {

        }; 
        private static readonly Dictionary<string, Func<string?>> _replacementsForVariables = new()
        {
            ["$seqlastloc"] = () => LastSeqIndex.ToString(),
            ["$seqlastlen"] = () => LastSeqLength.ToString(),
            ["$hdrlastloc"] = () => LastHeaderMetadataIndex.ToString(),
            ["$pnum"] = () => PileIndex.ToString(),
            ["$fulltime"] = () => DateTime.Now.ToString("HH-mm-ss"),
            ["$date"] = () => DateTime.Now.ToString("yyyy-MM-dd"),
            ["$fulldate"] = () => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
            ["$isodate"] = () => DateTime.Now.ToString("yyyyMMdd"),
            ["$isotime"] = () => DateTime.Now.ToString("HHmmss"),
            ["$timestamp"] = () => DateTime.Now.ToString("yyyyMMddHHmmss"),
            ["$timeU"] = () => DateTime.Now.ToString("HH:mm:ss"),
            ["$datetimeU"] = () => DateTime.Now.ToString("G"),
            ["$timestamp"] = () => DateTime.Now.Ticks.ToString(),
            ["$timeh"] = () => DateTime.Now.Hour.ToString(),
            ["$timemin"] = () => DateTime.Now.Minute.ToString(),
            ["$timesec"] = () => DateTime.Now.Second.ToString(),
            ["$timems"] = () => DateTime.Now.Millisecond.ToString(),

            ["$culture"] = () => CultureInfo.CurrentCulture.Name,
            ["$uiculture"] = () => CultureInfo.CurrentUICulture.Name,
            ["$language"] = () => CultureInfo.CurrentCulture.EnglishName,
            ["$timezone"] = () => TimeZoneInfo.Local.DisplayName,
            ["$timezoneoffset"] = () => TimeZoneInfo.Local.BaseUtcOffset.TotalHours.ToString(),

            ["$user"] = () => Environment.UserName,
            ["$pc"] = () => Environment.MachineName,
            ["$os"] = () => Environment.OSVersion.ToString(),
            ["$cpuc"] = () => Environment.ProcessorCount.ToString(),

            ["$home"] = () => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ["$desktop"] = () => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            ["$docs"] = () => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            ["$windownloads"] = () => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            ["$appdata"] = () => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ["$localappdata"] = () => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ["$tempp"] = () => Path.GetTempPath(),
            ["$tempf"] = () => Path.GetTempFileName(),
            ["$appdir"] = () => Process.GetCurrentProcess().MainModule?.FileName,
            ["$appfolder"] = () => AppContext.BaseDirectory,

            ["$localhost"] = () => Dns.GetHostName(),
            ["$ip"] = () => Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString(),
            ["$mac"] = () => NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault(),

            ["$rndN"] = () => CommandParser.Random.Next().ToString(),
            ["$rndS"] = () => CommandParser.Random.Next(1, int.MaxValue).ToString(),
            ["$rndU"] = () => CommandParser.Random.Next(int.MinValue, 0).ToString(),
            ["$rndperc"] = () => CommandParser.Random.Next(101).ToString() + "%",
            ["$rnd1h"] = () => CommandParser.Random.Next(101).ToString(),
            ["$rnd1t"] = () => CommandParser.Random.Next(1001).ToString(),
            ["$rndfl"] = () => CommandParser.Random.NextDouble().ToString("F4"),

            ["$fibonacci"] = () => {
                int a = 0, b = 1;
                for (int i = 0; i < Random.Shared.Next(10, 20); i++) (a, b) = (b, a + b);
                return a.ToString();
            },
            ["$trinum"] = () => {
                int n = Random.Shared.Next(2, 20);
                return ((n * (n + 1)) / 2).ToString();
            },
            ["$pi"] = () => Math.PI.ToString("F15"),
            ["$e"] = () => Math.E.ToString("F15"),

            // Application info (may change between versions)
            ["$metasig"] = () => PngSequenceFile.FileHeader.MetadataSignature,
            ["$hdrsig"] = () => PngSequenceFile.FileHeader.Signature,
            ["$hdrsize"] = () => PngSequenceFile.FileHeader.Size.ToString(),
            ["$asciiarts"] = () => ASCIIStuffFile.Count.ToString(),
            ["$appauthor"] = () => AppInformation.Author,
            ["$appname"] = () => AppInformation.FullName,
            ["$appversion"] = () => AppInformation.Version,
            ["$appdesc"] = () => AppInformation.Description,
            ["$github"] = () => AppInformation.GithubURL,
        };
        public static bool DeclareCustomVariable(string name, object? value)
        {
            string prefix = "&";
            string prefixedName = prefix + name;
            if (name.Contains(prefix) || name.Contains("$") || name.Contains("#") || name.Contains("[") || name.Contains("]"))
            {
                return false;
            }
            if (!_customDeclarations.ContainsKey(prefixedName))
            {
                _customDeclarations.Add(prefixedName, () => value);
            }
            else
            {
                _customDeclarations[prefixedName] = () => value;
            }
            return true;
        }
        public static object? GetCustomVariable(string name)
        {
            if(_customDeclarations.TryGetValue(name, out Func<object?>? func))
            {
                return func();
            }
            else
            {
                return null;
            }
        }
        public static object? GetCustomVariable(string name, out bool success)
        {
            object? value = GetCustomVariable(name);
            success = value != null;
            return value;
        }
        public static bool HasCustomVariable(string name)
        {
            return _customDeclarations.ContainsKey("&" + name);
        }
        public static bool RemoveCustomVariable(string name)
        {
            string prefixedName = "&" + name;
            if (_customDeclarations.ContainsKey(prefixedName))
            {
                _customDeclarations.Remove(prefixedName);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool GetCustomVariableFullInfo(string name, out (Type? Type, object? Value) tuple)
        {
            Func<object?>? valueFunc;
            bool success = _customDeclarations.TryGetValue(name, out valueFunc);
            object? val = valueFunc?.Invoke();
            tuple = new(val?.GetType(), val);

            return success;
        }
        public static void Parse(ref string input, bool variables)
        {
            Dictionary<string, Func<string?>> dict = variables ? _replacementsForVariables : _replacementsForAll;
            if (!string.IsNullOrEmpty(input))
            {
                StringBuilder builder = new StringBuilder(input);
                foreach (var replacement in dict)
                {
                    builder.Replace(replacement.Key, replacement.Value());
                }
                foreach (var customDeclaration in _customDeclarations)
                {
                    builder.Replace(customDeclaration.Key, customDeclaration.Value()?.ToString());
                }
                input = builder.ToString();
            }
            if (HasAnsiFormatting(ref input))
            {
                input += "\x1b[0m";
            }
        }
        public static bool HasAnsiFormatting(ref string input)
        {
            return Regex.IsMatch(input, @"\x1B\[[0-9;]*[mGKH]");
        }

        public static string[] ExpandSequencePath(string pattern)
        {
            // 1. FFmpeg-style with range (%04d[1>100])
            if (TryFfmpegRangePattern(pattern, out var ffmpegRangeResults))
                return ffmpegRangeResults;

            // 2. Houdini-style with range (####[1>50])
            if (TryHoudiniPattern(pattern, out var houdiniRangeResults))
                return houdiniRangeResults;

            // 3. Angle-bracket range (<0-48>)
            if (TryAngleBracketRange(pattern, out var angleBracketResults))
                return angleBracketResults;

            // 4. Bracket ranges (image_[1-5].png or image_[A-D].png)
            if (TryBracketRange(pattern, out var rangeResults))
                return rangeResults;

            // 5. Wildcards (image_*.png)
            if (pattern.Contains('*') || pattern.Contains('?'))
                return ExpandWildcard(pattern);

            // 6. Comma-separated lists (image_{1,3,5}.png)
            if (TryCommaSeparated(pattern, out var commaResults))
                return commaResults;

            // Fallback: Return as single path
            return [pattern];
        }

        private static bool TryFfmpegRangePattern(string pattern, out string[] results)
        {
            var match = Regex.Match(pattern, @"(%(\d*)d)\[(\d+)>(\d+)\]");
            if (!match.Success)
            {
                results = Array.Empty<string>();
                return false;
            }

            int padding = string.IsNullOrEmpty(match.Groups[2].Value) ? 1 : int.Parse(match.Groups[2].Value);
            int start = int.Parse(match.Groups[3].Value);
            int end = int.Parse(match.Groups[4].Value);

            // Get the base pattern without range specifier
            string basePattern = pattern.Replace(match.Groups[0].Value, match.Groups[1].Value);

            results = Enumerable.Range(start, end - start + 1)
                .Select(i => basePattern.Replace(match.Groups[1].Value, i.ToString($"D{padding}")))
                .ToArray();
            return true;
        }
        private static bool TryHoudiniPattern(string pattern, out string[] results)
        {
            // Match patterns like: 
            // bgeo.####[5>7].bgeo.sc
            // file_###[1>3].exr
            var match = Regex.Match(pattern, @"(#+)\[(\d+)>(\d+)\]");
            if (!match.Success)
            {
                results = Array.Empty<string>();
                return false;
            }

            int padding = match.Groups[1].Length;
            int start = int.Parse(match.Groups[2].Value);
            int end = int.Parse(match.Groups[3].Value);

            // Get the position of the pattern
            int patternPos = match.Index;

            // Rebuild the base path without the range specifier
            string basePath = pattern.Substring(0, patternPos) +
                             match.Groups[1].Value +
                             pattern.Substring(patternPos + match.Length);

            results = Enumerable.Range(start, end - start + 1)
                .Select(i => basePath.Replace(match.Groups[1].Value, i.ToString($"D{padding}")))
                .ToArray();
            return true;
        }
        private static bool TryAngleBracketRange(string pattern, out string[] results)
        {
            var match = Regex.Match(pattern, @"<(\d+)-(\d+)>");
            if (!match.Success)
            {
                results = Array.Empty<string>();
                return false;
            }

            int start = int.Parse(match.Groups[1].Value);
            int end = int.Parse(match.Groups[2].Value);

            results = Enumerable.Range(start, end - start + 1)
                .Select(i => pattern.Replace(match.Value, i.ToString()))
                .ToArray();
            return true;
        }
        private static bool TryBracketRange(string pattern, out string[] results)
        {
            var match = Regex.Match(pattern, @"\[(\w+)-(\w+)\]");
            if (!match.Success)
            {
                results = Array.Empty<string>();
                return false;
            }

            if (int.TryParse(match.Groups[1].Value, out int start) &&
                int.TryParse(match.Groups[2].Value, out int end))
            {
                // Numeric range
                results = Enumerable.Range(start, end - start + 1)
                    .Select(i => pattern.Replace(match.Value, i.ToString()))
                    .ToArray();
            }
            else if (match.Groups[1].Value.Length == 1 &&
                     match.Groups[2].Value.Length == 1)
            {
                // Character range
                char startChar = match.Groups[1].Value[0];
                char endChar = match.Groups[2].Value[0];
                results = Enumerable.Range(startChar, endChar - startChar + 1)
                    .Select(c => pattern.Replace(match.Value, ((char)c).ToString()))
                    .ToArray();
            }
            else
            {
                results = Array.Empty<string>();
                return false;
            }

            return true;
        }
        private static bool TryCommaSeparated(string pattern, out string[] results)
        {
            var match = Regex.Match(pattern, @"\{([^}]+)\}");
            if (!match.Success)
            {
                results = Array.Empty<string>();
                return false;
            }

            results = match.Groups[1].Value.Split(',')
                .Select(v => pattern.Replace(match.Value, v.Trim()))
                .ToArray();
            return true;
        }
        private static string[] ExpandWildcard(string pattern)
        {
            string dir = Path.GetDirectoryName(pattern) ?? ".";
            string searchPattern = Path.GetFileName(pattern);

            return Directory.Exists(dir) ?
                Directory.GetFiles(dir, searchPattern) :
                Array.Empty<string>();
        }

    }
}
