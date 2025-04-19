using System;
using System.Diagnostics.Metrics;
using System.IO;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;

namespace Blayms.PNGS.Constructor.Commands
{
    public class NewPNGSFileCommand : CommandBlockBase
    {
        public override string Name => "newpngs";
        public override string Description => "Command block that creates a *.pngs file!";
        public override string ExitCommandName => "exit";

        internal PngSequenceFile? file;
        internal bool preferMaximizedOnHeader;
        internal bool fileWasLoaded;

        protected override void OnRegistered()
        {
            SubjugatedCommands =
            [
                new SavePNGSFileCommand(this),
                new LoadPNGSFileCommand(this),
                new FlushPNGSFileCommand(this),
                new MakeSequenceElementCommand(this),
                new ModifySequenceElementAllCommand(this),
                new ModifySequenceElementImageCommand(this),
                new ModifySequenceElementLengthCommand(this),
                new ModifyHeaderAllCommand(this),
                new ModifyHeaderLoopCountCommand(this),
                new ModifyHeaderVersionCommand(this),
                new AddMetadataCommand(this),
                new RemoveMetadataCommand(this),
                new RemoveMetadataAtCommand(this),
                new SwapSequenceElementsCommand(this),
                new PullSequenceElementImageCommand(this),
                new RemoveSequenceElementAtCommand(this),
                new RemoveSequenceElementFirstCommand(this),
                new RemoveSequenceElementLastCommand(this),
                new SequenceElementLogCommand(this),
                new HeaderLogCommand(this),
                new HeaderMetadataLogCommand(this),
            ];
        }
        public override void Execute((Type, object?)[]? args, out bool fail)
        {
            ConsoleEx.WriteLine("Welcome to PNGS File creation! Please, use \"help\" command", ConsoleColor.Green);
            file = new PngSequenceFile();
            base.Execute(args, out fail);
        }
        public override void OnExit(ref bool keepBlockRunning)
        {
            base.OnExit(ref keepBlockRunning);
            ConsoleEx.WriteLine("Exited the PNGS file Creation block!", ConsoleColor.Green);
            CommandStringArgumentReplaceBuffer.LastSeqIndex = 0;
            CommandStringArgumentReplaceBuffer.LastSeqLength = 0;
            CommandStringArgumentReplaceBuffer.LastHeaderMetadataIndex = 0;
            fileWasLoaded = false;
            file = null;
        }
        public void RefreshVariables()
        {
            CommandStringArgumentReplaceBuffer.LastSeqIndex = file!.Count - 1;
            if (CommandStringArgumentReplaceBuffer.LastSeqIndex != -1)
            {
                CommandStringArgumentReplaceBuffer.LastSeqLength = (int)file.Sequences[file!.Count - 1].Length;
            }
            if (file!.Header != null)
            {
                CommandStringArgumentReplaceBuffer.LastHeaderMetadataIndex = file!.Header.GetMetadataCount() - 1;
            }
        }
        public class MakeSequenceElementCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "mkseq";
            public override string Description => "Creates a sequence element";
            public MakeSequenceElementCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("pngPath", (typeof(string), false, string.Empty)),
                    ("msLength", (typeof(int), false, 1000))
                );
                Flags =
                [
                    new CommandFlag("regex")
                ];
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                string pngPath = ExpectArgumentInstance<string>(ref args, 0, ref fail);
                int msLength = ExpectArgumentInstance<int>(ref args, 1, ref fail);

                CommandParser.ValidateIntegerLength(ref msLength, 1, int.MaxValue, "Invalid value encountered", "msLength out of range",
                    "Index must be greater than 0. Defaulting to 1ms.", $"Index exceeds maximum value ({int.MaxValue}). Defaulting to {int.MaxValue}ms.");

                if (!fail)
                {
                    try
                    {
                        string[] pngPaths = [pngPath];
                        if (GetFlagByIndex(0)!.IsRaised)
                        {
                            pngPaths = CommandStringArgumentReplaceBuffer.ExpandSequencePath(pngPath);
                        }
                        for (int i = 0; i < pngPaths.Length; i++)
                        {
                            PngSequenceFile.SequenceElement sequenceElement = new PngSequenceFile.SequenceElement(File.ReadAllBytes(pngPaths[i]), (uint)msLength);
                            ActualOwner.file?.AddSequence(sequenceElement);
                            ConsoleEx.IndentLevel++;
                            ConsoleEx.WriteLine($"Sequence #{ActualOwner.file?.Sequences.Count - 1}: Length (ms): {sequenceElement?.Length}, Pixel Count: {sequenceElement?.PixelsCount}, Size: {sequenceElement?.ihdrChunk.Width}x{sequenceElement?.ihdrChunk.Height}");
                            ConsoleEx.WriteLine($"Bit Depth: {sequenceElement?.ihdrChunk.BitDepth}, Interlace Method: {sequenceElement?.ihdrChunk.InterlaceMethod}, Color Type: {sequenceElement?.ihdrChunk.ColorType}");
                            ConsoleEx.WriteLine($"Compression Method: {sequenceElement?.ihdrChunk.CompressionMethod}, Filter Method: {sequenceElement?.ihdrChunk.FilterMethod}");
                            ConsoleEx.WriteLineFmt($"[#blink][#green]Use modseq to modify sequence #{ActualOwner.file?.Sequences.Count - 1}");
                            ConsoleEx.WriteLine();
                            ConsoleEx.IndentLevel--;
                        }

                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }
                ActualOwner.RefreshVariables();
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class ModifySequenceElementAllCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "modseq";
            public override string Description => "Modifies existing sequence element";
            public ModifySequenceElementAllCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("index", (typeof(int), false, 0)),
                    ("pngPath", (typeof(string), false, string.Empty)),
                    ("msLength", (typeof(int), false, 1000))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int index = ExpectArgumentInstance<int>(ref args, 0, ref fail);
                string pngPath = ExpectArgumentInstance<string>(ref args, 1, ref fail);
                int msLength = ExpectArgumentInstance<int>(ref args, 2, ref fail);

                CommandParser.ValidateIntegerLength(ref msLength, 1, int.MaxValue, "Invalid value encountered", "msLength out of range",
                    "Length must be greater than 0. Defaulting to 1ms.", $"Length exceeds maximum value ({int.MaxValue}). Defaulting to {int.MaxValue}ms.");
                if (!fail)
                {
                    fail = !CommandParser.ValidateIntegerLength(ref index, 0, ActualOwner.file!.Sequences.Count - 1, "Invalid value encountered", "Index out of range",
                        "Index must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Sequences.Count - 1})", false);
                }

                if (!fail)
                {
                    try
                    {
                        PngSequenceFile.SequenceElement sequenceElement = new PngSequenceFile.SequenceElement(File.ReadAllBytes(pngPath), (uint)msLength);
                        ActualOwner.file.Sequences[index] = sequenceElement;
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLine("Modifed successfully!", ConsoleColor.DarkGreen);
                        ConsoleEx.WriteLine($"Sequence #{index}: Length (ms): {sequenceElement?.Length}, Pixel Count: {sequenceElement?.PixelsCount}, Size: {sequenceElement?.ihdrChunk.Width}x{sequenceElement?.ihdrChunk.Height}");
                        ConsoleEx.WriteLine($"Bit Depth: {sequenceElement?.ihdrChunk.BitDepth}, Interlace Method: {sequenceElement?.ihdrChunk.InterlaceMethod}, Color Type: {sequenceElement?.ihdrChunk.ColorType}");
                        ConsoleEx.WriteLine($"Compression Method: {sequenceElement?.ihdrChunk.CompressionMethod}, Filter Method: {sequenceElement?.ihdrChunk.FilterMethod}");
                        ConsoleEx.IndentLevel--;
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }
                ActualOwner.RefreshVariables();
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class LoadPNGSFileCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "load";
            public override string Description => "Loads all PNGS data from a file path";
            public LoadPNGSFileCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("path", (typeof(string), false, string.Empty))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                string path = ExpectArgumentInstance<string>(ref args, 0, ref fail);

                if(string.IsNullOrEmpty(path))
                {
                    ConsoleEx.WriteError("Failed loading PNGS file", "Invalid path", "Path is either null or empty!");
                    fail = true;
                }

                if (!fail && !path.EndsWith(".pngs"))
                {
                    ConsoleEx.WriteError("Failed loading PNGS file", "Invalid path", "Path is either not a file or not a *.pngs file!");
                    fail = true;
                }

                if (!fail)
                {
                    try
                    {
                        ActualOwner.file = new PngSequenceFile(path);
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLine($"Successfully loaded and replaced PNGS File from {path}!", ConsoleColor.DarkGreen);
                        ConsoleEx.IndentLevel--;
                        ActualOwner.RefreshVariables();
                        Reset();
                        ActualOwner.fileWasLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }
            }
            protected override void Reset()
            {

            }
        }
        public class FlushPNGSFileCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "flush";
            public override string Description => "Clears all existing concurrent PNGS data";
            public FlushPNGSFileCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                Flags =
                [
                    new CommandFlag("yes")
                ];
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                ConsoleEx.IndentLevel++;
                if (!GetFlagByIndex(0).IsRaised)
                {
                    ConsoleEx.WriteLineFmt("Are you sure about flushing all PNGS file data? ([#green][#blink]Y[blink#][clr#]/[#red][#blink]N[blink#][clr#])");
                    if (ConsoleEx.ReadKey().Key != ConsoleKey.Y)
                    {
                        fail = true;
                    }
                }

                if (!fail)
                {
                    ActualOwner.file = new PngSequenceFile();
                    if (!GetFlagByIndex(0).IsRaised)
                    {
                        Console.WriteLine();
                    }
                    ConsoleEx.WriteLineFmt("[#red][#blink]Reset all PNGS file data...[blink#][clr#]");
                }
                ConsoleEx.IndentLevel--;
                ActualOwner.RefreshVariables();
                ActualOwner.fileWasLoaded = false;
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class SavePNGSFileCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "save";
            public override string Description => "Saves a *.pngs file to a specified path";
            public SavePNGSFileCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("path", (typeof(string), false, string.Empty)),
                    ("preferMaximized", (typeof(bool), false, null!)),
                    ("loopCount", (typeof(int), true, -1)),
                    ("version", (typeof(int), true, 0))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                string path = ExpectArgumentInstance<string>(ref args, 0, ref fail);
                bool preferMaximized = ExpectArgumentInstance<bool>(ref args, 1, ref fail);
                int loopCount = ExpectArgumentInstance<int>(ref args, 2, ref fail);
                int version = ExpectArgumentInstance<int>(ref args, 3, ref fail);

                if(path == null)
                {
                    ConsoleEx.WriteError("Failed saving a PNGS File", "Invalid path", "Path is not a string value!");
                    fail = true;
                }
                if (!fail)
                {
                    if (!path.EndsWith(".pngs"))
                    {
                        ConsoleEx.WriteError("Failed saving a PNGS File", "Invalid path", "Path is either not a file or not a *.pngs file!");
                        fail = true;
                    }
                }
                if (ActualOwner.file!.Header == null)
                {
                    ActualOwner.file!.Header = new PngSequenceFile.FileHeader(ActualOwner.file, preferMaximized);
                    ActualOwner.preferMaximizedOnHeader = preferMaximized;
                }
                ActualOwner.file.Header.LoopCount = loopCount;
                ActualOwner.file.Header.Version = version;
                if (!ActualOwner.fileWasLoaded)
                {
                    ActualOwner.file.Header.Refresh(ActualOwner.preferMaximizedOnHeader);
                }
                if (!fail)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            using (PngSequenceFileWriter pngsWriter = new PngSequenceFileWriter(fs))
                            {
                                pngsWriter.Write(ActualOwner.file);
                                fs.Flush();

                                if (fs.Position > 0)
                                {
                                    ConsoleEx.IndentLevel++;
                                    ConsoleEx.WriteLine($"Successfully saved PNGS File to {path}!", ConsoleColor.DarkGreen);
                                    ConsoleEx.IndentLevel--;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }

                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class ModifyHeaderAllCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "modhdr";
            public override string Description => "Modifies or creates the header";
            public ModifyHeaderAllCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("preferMaximized", (typeof(bool), false, null!)),
                    ("loopCount", (typeof(int), true, -1)),
                    ("version", (typeof(int), true, 0))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                bool preferMaximized = ExpectArgumentInstance<bool>(ref args, 0, ref fail);
                int loopCount = ExpectArgumentInstance<int>(ref args, 1, ref fail);
                int version = ExpectArgumentInstance<int>(ref args, 2, ref fail);
                bool hadToCreate = false;
                if (ActualOwner.file!.Header == null)
                {
                    ActualOwner.file!.Header = new PngSequenceFile.FileHeader(ActualOwner.file, preferMaximized);
                    ActualOwner.preferMaximizedOnHeader = preferMaximized;
                    hadToCreate = true;
                }

                if (!fail)
                {
                    try
                    {
                        ActualOwner.file.Header.LoopCount = loopCount;
                        ActualOwner.file.Header.Version = version;
                        if (!ActualOwner.fileWasLoaded)
                        {
                            ActualOwner.file.Header.Refresh(false);
                        }
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLine($"Successfully { (hadToCreate ? "created" : "modified") } the header!", ConsoleColor.DarkGreen);
                        ConsoleEx.IndentLevel--;
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }

                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class ModifyHeaderLoopCountCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "modhdrloops";
            public override string Description => "Modifies header's loop count value";
            public ModifyHeaderLoopCountCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("loopCount", (typeof(int), true, -1))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int loopCount = ExpectArgumentInstance<int>(ref args, 0, ref fail);
                bool hadToCreate = false;
                if (ActualOwner.file!.Header == null)
                {
                    ActualOwner.file!.Header = new PngSequenceFile.FileHeader(ActualOwner.file, ActualOwner.preferMaximizedOnHeader);
                    hadToCreate = true;
                }

                if (!fail)
                {
                    try
                    {
                        ActualOwner.file.Header!.LoopCount = loopCount;
                        if (!ActualOwner.fileWasLoaded)
                        {
                            ActualOwner.file.Header.Refresh(false);
                        }
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLine($"Successfully {(hadToCreate ? "created" : "modified")} the header!", ConsoleColor.DarkGreen);
                        ConsoleEx.IndentLevel--;
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }

                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class ModifyHeaderVersionCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "modhdrver";
            public override string Description => "Modifies header's version value";
            public ModifyHeaderVersionCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("version", (typeof(int), false, 0))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int version = ExpectArgumentInstance<int>(ref args, 0, ref fail);
                bool hadToCreate = false;
                if (ActualOwner.file!.Header == null)
                {
                    ActualOwner.file!.Header = new PngSequenceFile.FileHeader(ActualOwner.file, ActualOwner.preferMaximizedOnHeader);
                    hadToCreate = true;
                }

                if (!fail)
                {
                    try
                    {
                        ActualOwner.file.Header!.Version = version;
                        if (!ActualOwner.fileWasLoaded)
                        {
                            ActualOwner.file.Header.Refresh(false);
                        }
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLine($"Successfully {(hadToCreate ? "created" : "modified")} the header!", ConsoleColor.DarkGreen);
                        ConsoleEx.IndentLevel--;
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }

                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class SwapSequenceElementsCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "swap";
            public override string Description => "Swaps 2 sequence elements";
            public SwapSequenceElementsCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("a", (typeof(int), false, 0)),
                    ("b", (typeof(int), false, 0))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int a = ExpectArgumentInstance<int>(ref args, 0, ref fail);
                int b = ExpectArgumentInstance<int>(ref args, 1, ref fail);

                fail = !CommandParser.ValidateIntegerLength(ref a, 0, ActualOwner.file!.Sequences.Count - 1, "Invalid value encountered", "Index out of range",
                    "Index (a) must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Sequences.Count - 1}).", false);
                if (!fail)
                {
                    fail = !CommandParser.ValidateIntegerLength(ref b, 0, ActualOwner.file!.Sequences.Count - 1, "Invalid value encountered", "Index out of range",
                    "Index (b) must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Sequences.Count - 1}).", false);
                }
                if (!fail)
                {
                    try
                    {
                        ActualOwner.file!.Sequences.Swap(a, b);
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLineFmt($"[#green][#blink]Successfully swapped sequence element #{a} with sequence element #{b}![blink#][clr#]");
                        ConsoleEx.IndentLevel--;
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }

                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class PullSequenceElementImageCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "pullimgseq";
            public override string Description => "Converts a sequence element to *.png file and outputs it in the file path";
            public PullSequenceElementImageCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("index", (typeof(int), false, 0)),
                    ("outputPath", (typeof(string), false, string.Empty))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int index = ExpectArgumentInstance<int>(ref args, 0, ref fail);
                string outputPath = ExpectArgumentInstance<string>(ref args, 1, ref fail);

                if (!fail)
                {
                    fail = !CommandParser.ValidateIntegerLength(ref index, 0, ActualOwner.file!.Sequences.Count - 1, "Invalid value encountered", "Index out of range",
                        "Index must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Sequences.Count - 1})", false);
                }
                if(outputPath == null && !fail)
                {
                    fail = true;
                }
                if (!fail && !outputPath!.EndsWith(".png"))
                {
                    ConsoleEx.WriteError("Failed pulling image from sequence element", "Invalid path", "Path is either not a file or not a *.png file!");
                    fail = true;
                }

                if(ActualOwner.file.Header == null && !fail)
                {
                    ConsoleEx.WriteError("Failed pulling image from sequence element", "Header was never created", "Use \"save\" or \"modhdr\" commands to setup up the header!");
                    fail = true;
                }

                if (!fail)
                {
                    try
                    {
                        File.WriteAllBytes(outputPath, ActualOwner?.file.Sequences[index].EncodeToPNG()!);
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLine($"Pulled image to {outputPath} successfully!", ConsoleColor.DarkGreen);
                        ConsoleEx.IndentLevel--;
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }

                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class ModifySequenceElementImageCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "modseqimg";
            public override string Description => "Modifies an image of a specified sequence element";
            public ModifySequenceElementImageCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("index", (typeof(int), false, 0)),
                    ("pngPath", (typeof(string), false, string.Empty))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int index = ExpectArgumentInstance<int>(ref args, 0, ref fail);
                string pngPath = ExpectArgumentInstance<string>(ref args, 1, ref fail);

                if (!fail)
                {
                    fail = !CommandParser.ValidateIntegerLength(ref index, 0, ActualOwner.file!.Sequences.Count - 1, "Invalid value encountered", "Index out of range",
                        "Index must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Sequences.Count - 1})", false);
                }

                if (!fail)
                {
                    try
                    {
                        PngSequenceFile.SequenceElement sequenceElement = ActualOwner.file.Sequences[index];
                        sequenceElement.SwapPng(pngPath);
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLine("Modifed image successfully!", ConsoleColor.DarkGreen);
                        ConsoleEx.WriteLine($"Sequence #{index}: Length (ms): {sequenceElement?.Length}, Pixel Count: {sequenceElement?.PixelsCount}, Size: {sequenceElement?.ihdrChunk.Width}x{sequenceElement?.ihdrChunk.Height}");
                        ConsoleEx.WriteLine($"Bit Depth: {sequenceElement?.ihdrChunk.BitDepth}, Interlace Method: {sequenceElement?.ihdrChunk.InterlaceMethod}, Color Type: {sequenceElement?.ihdrChunk.ColorType}");
                        ConsoleEx.WriteLine($"Compression Method: {sequenceElement?.ihdrChunk.CompressionMethod}, Filter Method: {sequenceElement?.ihdrChunk.FilterMethod}");
                        ConsoleEx.IndentLevel--;
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }

                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class ModifySequenceElementLengthCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "modseqlen";
            public override string Description => "Modifies length / duration of a specified sequence element";
            public ModifySequenceElementLengthCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("index", (typeof(int), false, 0)),
                    ("msLength", (typeof(int), false, 1000))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int index = ExpectArgumentInstance<int>(ref args, 0, ref fail);
                int msLength = ExpectArgumentInstance<int>(ref args, 1, ref fail);

                CommandParser.ValidateIntegerLength(ref msLength, 1, int.MaxValue, "Invalid value encountered", "msLength out of range",
                    "Length must be greater than 0. Defaulting to 1ms.", $"Length exceeds maximum value ({int.MaxValue}). Defaulting to {int.MaxValue}ms.");

                fail = !CommandParser.ValidateIntegerLength(ref index, 0, ActualOwner.file!.Sequences.Count - 1, "Invalid value encountered", "Index out of range",
                    "Index must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Sequences.Count - 1})", false);

                if (!fail)
                {
                    try
                    {
                        PngSequenceFile.SequenceElement sequenceElement = ActualOwner.file.Sequences[index];
                        sequenceElement.Length = (uint)msLength;
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLine("Modifed length successfully!", ConsoleColor.DarkGreen);
                        ConsoleEx.WriteLine($"Sequence #{index}: Length (ms): {sequenceElement?.Length}, Pixel Count: {sequenceElement?.PixelsCount}, Size: {sequenceElement?.ihdrChunk.Width}x{sequenceElement?.ihdrChunk.Height}");
                        ConsoleEx.WriteLine($"Bit Depth: {sequenceElement?.ihdrChunk.BitDepth}, Interlace Method: {sequenceElement?.ihdrChunk.InterlaceMethod}, Color Type: {sequenceElement?.ihdrChunk.ColorType}");
                        ConsoleEx.WriteLine($"Compression Method: {sequenceElement?.ihdrChunk.CompressionMethod}, Filter Method: {sequenceElement?.ihdrChunk.FilterMethod}");
                        ConsoleEx.IndentLevel--;
                    }
                    catch (Exception ex)
                    {
                        ConsoleEx.WriteError("Caught an exception from Blayms.PNGS", $"{ex.GetType().Name}", $"{ex.Message}");
                    }
                }
                ActualOwner.RefreshVariables();
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class RemoveSequenceElementAtCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "delseq";
            public override string Description => "Deletes a sequence element by index";
            public RemoveSequenceElementAtCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("index", (typeof(int), false, 0))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int index = ExpectArgumentInstance<int>(ref args, 0, ref fail);
                if (!fail)
                {
                    fail = !CommandParser.ValidateIntegerLength(ref index, 0, ActualOwner.file!.Sequences.Count - 1, "Invalid value encountered", "Index out of range",
                        "Index must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Sequences.Count - 1})", false);
                }

                if (!fail)
                {
                    ActualOwner.file!.Sequences.RemoveAt(index);
                    ConsoleEx.BeginForegroundColor(ConsoleColor.DarkGreen);
                    ConsoleEx.WriteLineFmt($"Successfully removed sequence element #{index}! [#blink]Count = {ActualOwner.file!.Sequences.Count}[blink#]");
                    ConsoleEx.EndForegroundColor();
                }
                ActualOwner.RefreshVariables();
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class RemoveSequenceElementFirstCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "delseqf";
            public override string Description => "Deletes the first sequence element";
            public RemoveSequenceElementFirstCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int index = 0;
                if (!fail)
                {
                    fail = !CommandParser.ValidateIntegerLength(ref index, 0, ActualOwner.file!.Sequences.Count - 1, "Invalid value encountered", "Index out of range",
                        "Index must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Sequences.Count - 1})", false);
                }

                if (!fail)
                {
                    ActualOwner.file!.Sequences.RemoveAt(index);
                    ConsoleEx.BeginForegroundColor(ConsoleColor.DarkGreen);
                    ConsoleEx.WriteLineFmt($"Successfully removed the first sequence element! [#blink]Count = {ActualOwner.file!.Sequences.Count}[blink#]");
                    ConsoleEx.EndForegroundColor();
                }
                ActualOwner.RefreshVariables();
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class RemoveSequenceElementLastCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "delseql";
            public override string Description => "Deletes the last sequence element";
            public RemoveSequenceElementLastCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);
                int index = ActualOwner.file!.Sequences.Count - 1;
                if (!fail)
                {
                    fail = !CommandParser.ValidateIntegerLength(ref index, 0, ActualOwner.file!.Sequences.Count - 1, "Invalid value encountered", "Index out of range",
                        "Index must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Sequences.Count - 1})", false);
                }

                if (!fail)
                {
                    ActualOwner.file!.Sequences.RemoveAt(index);
                    ConsoleEx.BeginForegroundColor(ConsoleColor.DarkGreen);
                    ConsoleEx.WriteLineFmt($"Successfully removed the last sequence element! [#blink]Count = {ActualOwner.file!.Sequences.Count}[blink#]");
                    ConsoleEx.EndForegroundColor();
                }
                ActualOwner.RefreshVariables();
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class AddMetadataCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "mkmeta";
            public override string Description => "Adds a metadata entry to header";
            public AddMetadataCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("value", (typeof(string), false, 0))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);

                string value = ExpectArgumentInstance<string>(ref args, 0, ref fail);

                if (ActualOwner.file!.Header == null && !fail)
                {
                    ConsoleEx.WriteError("Failed adding header metadata", "Header was never created", "Use \"save\" or \"modhdr\" commands to setup up the header!");
                    fail = true;
                }

                if (!fail)
                {
                    ConsoleEx.WriteLine();
                    ConsoleEx.IndentLevel++;
                    if (ActualOwner.file!.Header!.ContainsMetadata(value))
                    {
                        ConsoleEx.WriteLine($"Header already contains \"{value}\" entry!", ConsoleColor.Yellow);
                    }
                    else
                    {
                        ConsoleEx.WriteLine($"Successfully added \"{value}\" entry at #{ActualOwner.file!.Header!.GetMetadataCount()}!", ConsoleColor.DarkGreen);
                    }
                    ActualOwner.file!.Header!.AddMetadata(value);
                    ConsoleEx.IndentLevel--;
                }
                ActualOwner.RefreshVariables();
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class RemoveMetadataCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "delmeta";
            public override string Description => "Removes a metadata entry from header (through value)";
            public RemoveMetadataCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("value", (typeof(string), false, 0))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);

                string value = ExpectArgumentInstance<string>(ref args, 0, ref fail);

                if (ActualOwner.file!.Header == null && !fail)
                {
                    ConsoleEx.WriteError("Failed deleting header metadata", "Header was never created", "Use \"save\" or \"modhdr\" commands to setup up the header!");
                    fail = true;
                }

                if (!fail)
                {
                    ConsoleEx.WriteLine();
                    ConsoleEx.IndentLevel++;
                    if (!ActualOwner.file!.Header!.ContainsMetadata(value))
                    {
                        ConsoleEx.WriteLine($"Header does not have \"{value}\" entry!", ConsoleColor.Yellow);
                    }
                    else
                    {
                        ConsoleEx.WriteLine($"Successfully removed \"{value}\" entry!", ConsoleColor.DarkGreen);
                    }
                    ActualOwner.file!.Header!.RemoveMetadata(value);
                    ConsoleEx.IndentLevel--;
                }
                ActualOwner.RefreshVariables();
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class RemoveMetadataAtCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "idelmeta";
            public override string Description => "Removes a metadata entry from header (through index)";
            public RemoveMetadataAtCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("index", (typeof(int), false, 0))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);

                int index = ExpectArgumentInstance<int>(ref args, 0, ref fail);

                if (ActualOwner.file!.Header == null && !fail)
                {
                    ConsoleEx.WriteError("Failed deleting header metadata", "Header was never created", "Use \"save\" or \"modhdr\" commands to setup up the header!");
                    fail = true;
                }
                if (!fail)
                {
                    CommandParser.ValidateIntegerLength(ref index, 0, ActualOwner.file!.Header!.GetMetadataCount() - 1, "Command execution ignored", "Index was out of range", "less than 0", $"greater than {ActualOwner.file!.Header!.GetMetadataCount() - 1}", true);
                }

                if (!fail)
                {
                    ConsoleEx.WriteLine();
                    ConsoleEx.IndentLevel++;
                    if (ActualOwner.file!.Header!.IsValidMetadataIndex(index))
                    {
                        ConsoleEx.IndentLevel++;
                        ConsoleEx.WriteLine($"Successfully removed metadata entry #{index}!", ConsoleColor.DarkGreen);
                        ConsoleEx.IndentLevel--;
                    }
                    ActualOwner.file!.Header!.RemoveMetadataAt(index);
                    ConsoleEx.IndentLevel--;
                }
                ActualOwner.RefreshVariables();
                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class HeaderLogCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "loghdr";
            public override string Description => "Prints information about the header";
            public HeaderLogCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {

            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);

                if (!fail)
                {
                    ConsoleEx.WriteLine();
                    ConsoleEx.IndentLevel++;
                    if (ActualOwner.file!.Header != null)
                    {
                        ConsoleEx.WriteLine($"Version = {ActualOwner.file!.Header.Version}, Loop Count = {ActualOwner.file!.Header.LoopCount}, Size: {ActualOwner.file!.Header.IHDR.Width}x{ActualOwner.file!.Header.IHDR.Height}");
                        ConsoleEx.WriteLine($"Bit Depth: {ActualOwner.file!.Header.IHDR.BitDepth}, Interlace Method: {ActualOwner.file!.Header.IHDR.InterlaceMethod}, Color Type: {ActualOwner.file!.Header.IHDR.ColorType}");
                        ConsoleEx.WriteLine($"Compression Method: {ActualOwner.file!.Header.IHDR.CompressionMethod}, Filter Method: {ActualOwner.file!.Header.IHDR.FilterMethod}");
                    }
                    else
                    {
                        ConsoleEx.WriteError("Failed gathering header information", "Header was never created", "Please use \"save\" command to generate a proper header!");
                    }
                    ConsoleEx.IndentLevel--;
                }

                Reset();
            }
            protected override void Reset()
            {

            }
        }
        public class SequenceElementLogCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "logseq";
            public override string Description => "Prints information about a certain sequence element";
            public SequenceElementLogCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("index", (typeof(int), true, -1))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);

                int index = ExpectArgumentInstance<int>(ref args, 0, ref fail);

                if (!fail)
                {
                    ConsoleEx.WriteLine();
                    ConsoleEx.IndentLevel++;
                    if (index == -1)
                    {
                        if (ActualOwner.file?.Sequences.Count == 0)
                        {
                            ConsoleEx.WriteLine("No sequences found.", ConsoleColor.Yellow);
                        }
                        for (int i = 0; i < ActualOwner.file?.Sequences.Count; i++)
                        {
                            PngSequenceFile.SequenceElement? seq = ActualOwner.file?.Sequences[i];
                            LogSequence(seq, i);
                        }
                    }
                    else
                    {
                        if (ActualOwner.file?.Sequences == null || index < 0 || index >= ActualOwner.file.Sequences.Count)
                        {
                            ConsoleEx.WriteError("Error gathering sequence", "Index was out of range", $"{index} was either equal, greater than or less than {ActualOwner.file?.Sequences.Count}", true);
                        }
                        else
                        {
                            LogSequence(ActualOwner.file?.Sequences[index], index);
                        }
                    }
                    ConsoleEx.IndentLevel--;
                }

                Reset();
            }
            private void LogSequence(PngSequenceFile.SequenceElement? seq, int index)
            {
                ConsoleEx.WriteLine($"Sequence #{index}: Length (ms): {seq?.Length}, Pixel Count: {seq?.PixelsCount}, Size: {seq?.ihdrChunk.Width}x{seq?.ihdrChunk.Height}");
                ConsoleEx.WriteLine($"Bit Depth: {seq?.ihdrChunk.BitDepth}, Interlace Method: {seq?.ihdrChunk.InterlaceMethod}, Color Type: {seq?.ihdrChunk.ColorType}");
                ConsoleEx.WriteLine($"Compression Method: {seq?.ihdrChunk.CompressionMethod}, Filter Method: {seq?.ihdrChunk.FilterMethod}");
                ConsoleEx.WriteLine();
            }
            protected override void Reset()
            {

            }
        }
        public class HeaderMetadataLogCommand : SubCommandBase<NewPNGSFileCommand>
        {
            public override string Name => "logmeta";
            public override string Description => "Prints a metadata entry value";
            public HeaderMetadataLogCommand(NewPNGSFileCommand? owner) : base(owner) { }

            protected override void OnRegistered()
            {
                ArgumentInfo = new
                (
                    ("index", (typeof(int), true, -1))
                );
            }


            public override void Execute((Type, object?)[]? args, out bool fail)
            {
                base.Execute(args, out fail);

                int index = ExpectArgumentInstance<int>(ref args, 0, ref fail);

                if (ActualOwner.file!.Header == null && !fail)
                {
                    ConsoleEx.WriteError("Failed logging header metadata", "Header was never created", "Use \"save\" or \"modhdr\" commands to setup up the header!");
                    fail = true;
                }

                if (!fail)
                {
                    fail = !CommandParser.ValidateIntegerLength(ref index, -1, ActualOwner.file!.Header!.GetMetadataCount() - 1, "Invalid value encountered", "Index out of range",
                    "Index must be greater than 0.", $"Index exceeds maximum value ({ActualOwner.file!.Header.GetMetadataCount() - 1})", false);
                }

                if (!fail)
                {
                    ConsoleEx.WriteLine();
                    ConsoleEx.IndentLevel++;
                    if (index == -1)
                    {
                        if (ActualOwner.file?.Header!.GetMetadataCount() == 0)
                        {
                            ConsoleEx.WriteLine("No metadata entries found.", ConsoleColor.Yellow);
                        }
                        IEnumerator<string>? allMetadataEntries = ActualOwner.file?.Header!.GetMetadataEnumerator();
                        int iterationIndex = 0;
                        if (allMetadataEntries != null)
                        {
                            while (allMetadataEntries.MoveNext())
                            {
                                string currentMetadata = allMetadataEntries.Current;
                                ConsoleEx.WriteLine($"Header Metadata Entry #{iterationIndex}:");
                                ConsoleEx.WriteLine(currentMetadata);
                                ConsoleEx.WriteLine();
                                iterationIndex++;
                            }
                        }
                    }
                    else
                    {
                        string metadata = ActualOwner.file!.Header!.GetMetadataAt(index);

                        ConsoleEx.WriteLine($"Header Metadata Entry #{index}:");
                        ConsoleEx.WriteLine(metadata);
                        ConsoleEx.WriteLine();
                    }

                    ConsoleEx.IndentLevel--;
                }

                Reset();
            }
            protected override void Reset()
            {

            }
        }
    }
}
