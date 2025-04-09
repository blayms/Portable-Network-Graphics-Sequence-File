using System.Text.Json;

namespace Blayms.PNGS.Test
{
    internal static class Program
    {
        private static void WriteColored(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== Starting PNG Sequence File Tests ===");
            Console.ResetColor();

            WriteColored("Before testing, would you like to provide a png file to create test *.pnga file and save it to Download? If not, type 'skip' to skip it", ConsoleColor.Cyan);
            string? input0 = Console.ReadLine();
            if (input0 != null && !input0.Equals("skip", StringComparison.OrdinalIgnoreCase))
            {
                CreateAndSaveToDownloads(input0);
            }


            TestEmptyFileCreation();
            TestFileHeaderSignature();
            TestSequenceOperations();
            TestConstructFromSequences();
            TestConstructFromPNGBytes();

            WriteColored("\n[NOTICE] Before testing serialization, please provide a *.png file path or type 'skip' to skip", ConsoleColor.Cyan);
            string? input1 = Console.ReadLine();
            if (input1 != null && !input1.Equals("skip", StringComparison.OrdinalIgnoreCase))
            {
                TestFileSerialization(input1);
            }

            TestEnumeratorBehavior();

            WriteColored("\n[NOTICE] Before testing png encoding, swapping on a first sequence and log loop count, please provide a *.png file path or type 'skip' to skip", ConsoleColor.Cyan);
            string? input2 = Console.ReadLine();
            if (input2 != null && !input2.Equals("skip", StringComparison.OrdinalIgnoreCase))
            {
                TestEncodingAndSwappingFirstSequenceToPNG(input2);
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== All Tests Completed ===");
            Console.ResetColor();
        }
        private static void CreateAndSaveToDownloads(string pngFile)
        {
            PngSequenceFile png = PngSequenceFile.ConstructFromPNGWithEqualDuration(false, 100,
                pngFile, pngFile, pngFile
                );
            using (FileStream fs = new FileStream(@$"C:\Users\{Environment.UserName}\Downloads\test.pngs", FileMode.OpenOrCreate))
            {
                using (PngSequenceFileWriter pngsW = new PngSequenceFileWriter(fs))
                {
                    pngsW.Write(png);
                }
            }
        }
        private static void TestEncodingAndSwappingFirstSequenceToPNG(string pngsPath)
        {
            WriteColored("\n[TEST] Testing encoding and swapping PNG in the first sequence...", ConsoleColor.White);
            PngSequenceFile pngFile = new PngSequenceFile(pngsPath);

            try
            {
                byte[] png = pngFile[0].EncodeToPNG();
                File.WriteAllBytes(@$"C:\Users\{Environment.UserName}\Downloads\pngtest.png", png);
                WriteColored(@$"[PASS] Tested encoding successfully. Final byte length: {png.Length}{Environment.NewLine}Check the file located at: C:\Users\{Environment.UserName}\Downloads\pngtest.png", ConsoleColor.Green);
            }
            catch(Exception ex)
            {
                WriteColored($"[FAIL] Failed to encode to PNG!\nException: {ex}", ConsoleColor.Red);
            }

            try
            {
                pngFile[0].SwapPng(pngFile[1].EncodeToPNG());
                WriteColored($"[PASS] Tested swapping successfully!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                WriteColored($"[FAIL] Failed to swap PNG from sequence #1 to sequence #0!\nException: {ex}", ConsoleColor.Red);
            }

            WriteColored($"[LOG] Loop Count: {pngFile.Header.LoopCount}!", ConsoleColor.DarkMagenta);
        }
        private static void TestEmptyFileCreation()
        {
            WriteColored("\n[TEST] Testing empty file creation...", ConsoleColor.White);

            var pngFile = new PngSequenceFile();
            Console.WriteLine($"- Initial Count: {pngFile.Count} (Expected: 0)");
            Console.WriteLine($"- Header null check: {pngFile.Header == null} (Expected: True)");
            Console.WriteLine($"- Sequences collection: {pngFile.Sequences?.Count ?? -1} (Expected: 0)");

            WriteColored("[PASS] Empty file test passed", ConsoleColor.Green);
        }

        private static void TestFileHeaderSignature()
        {
            WriteColored("\n[TEST] Testing file header signature validation...", ConsoleColor.White);

            try
            {
                byte[] invalidData = new byte[100];
                Array.Fill(invalidData, (byte)0);
                new PngSequenceFile(invalidData);
                WriteColored("[FAIL] Failed to catch invalid signature", ConsoleColor.Red);
            }
            catch (Exceptions.PNGSReadFailedException ex)
            {
                WriteColored($"[PASS] Correctly caught invalid signature: {ex.Message}", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                WriteColored($"[FAIL] Wrong exception type: {ex.GetType().Name}", ConsoleColor.Red);
            }
        }

        private static void TestSequenceOperations()
        {
            WriteColored("\n[TEST] Testing sequence operations...", ConsoleColor.White);

            var pngFile = new PngSequenceFile();
            var testSequence = new PngSequenceFile.SequenceElement();

            Console.WriteLine("- Testing AddSequence...");
            pngFile.AddSequence(testSequence);
            Console.WriteLine($"  Count after add: {pngFile.Count} (Expected: 1)");

            Console.WriteLine("- Testing ContainsSequence...");
            bool contains = pngFile.ContainsSequence(testSequence);
            Console.WriteLine($"  Contains added sequence: {contains} (Expected: True)");

            Console.WriteLine("- Testing RemoveSequence...");
            pngFile.RemoveSequence(testSequence);
            Console.WriteLine($"  Count after remove: {pngFile.Count} (Expected: 0)");

            WriteColored("[PASS] Sequence operations test passed", ConsoleColor.Green);
        }

        private static void TestConstructFromSequences()
        {
            WriteColored("\n[TEST] Testing ConstructFromSequences...", ConsoleColor.White);

            var sequences = new PngSequenceFile.SequenceElement[]
            {
                new PngSequenceFile.SequenceElement { Length = 100 },
                new PngSequenceFile.SequenceElement { Length = 200 }
            };

            Console.WriteLine("- Testing with preferMaximizedValues = true");
            var maxFile = PngSequenceFile.ConstructFromSequences(true, sequences);
            Console.WriteLine($"  Sequence count: {maxFile.Count} (Expected: 2)");
            Console.WriteLine($"  Header initialized: {maxFile.Header != null} (Expected: True)");

            Console.WriteLine("- Testing with preferMaximizedValues = false");
            var minFile = PngSequenceFile.ConstructFromSequences(false, sequences);
            Console.WriteLine($"  Sequence count: {minFile.Count} (Expected: 2)");

            WriteColored("[PASS] ConstructFromSequences test passed", ConsoleColor.Green);
        }

        private static void TestConstructFromPNGBytes()
        {
            WriteColored("\n[TEST] Testing ConstructFromPNGWithEqualDuration...", ConsoleColor.White);

            try
            {
                byte[] minimalPng = new byte[100];
                Array.Fill(minimalPng, (byte)0);

                var pngFile = PngSequenceFile.ConstructFromPNGWithEqualDuration(
                    preferMaximizedValues: true,
                    msDuration: 100,
                    minimalPng);

                Console.WriteLine($"- Created file with {pngFile.Count} sequences (Expected: 1)");
                Console.WriteLine($"- First sequence length: {pngFile.Sequences[0].Length} (Expected: 100)");

                WriteColored("[PASS] ConstructFromPNGWithEqualDuration test passed", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                WriteColored($"[FAIL] Exception during test: {ex.Message}", ConsoleColor.Red);
            }
        }

        private static void TestFileSerialization(string filePath)
        {
            WriteColored("\n[TEST] Testing file serialization...", ConsoleColor.White);

            try
            {
                var pngFile = new PngSequenceFile(filePath);

                // Serialize
                string json = JsonSerializer.Serialize(pngFile);
                Console.WriteLine($"- Serialized JSON size: {json.Length} bytes");
                WriteColored($"- Serialized JSON sample:\n{json.Substring(0, Math.Min(100, json.Length))}...", ConsoleColor.DarkGray);

                // Deserialize
                var deserialized = JsonSerializer.Deserialize<PngSequenceFile>(json);
                Console.WriteLine($"- Deserialized type: {deserialized?.GetType().Name ?? "null"} (Expected: PngSequenceFile)");
                Console.WriteLine($"- Deserialized sequence count: {deserialized?.Count ?? -1} (Expected: {pngFile.Count})");

                WriteColored("[PASS] File serialization test passed", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                WriteColored($"[FAIL] Serialization failed: {ex.Message}", ConsoleColor.Red);
            }
        }

        private static void TestEnumeratorBehavior()
        {
            WriteColored("\n[TEST] Testing enumerator behavior...", ConsoleColor.White);

            var pngFile = new PngSequenceFile();
            pngFile.AddRangeOfSequences(
                new PngSequenceFile.SequenceElement { Length = 100 },
                new PngSequenceFile.SequenceElement { Length = 200 });

            Console.WriteLine("- Testing foreach enumeration...");
            int count = 0;
            foreach (var seq in pngFile)
            {
                Console.WriteLine($"  Sequence {++count}: Length={seq.Length}");
            }
            Console.WriteLine($"  Total sequences enumerated: {count} (Expected: 2)");

            Console.WriteLine("- Testing manual enumeration...");
            count = 0;
            var enumerator = pngFile.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Console.WriteLine($"  Sequence {++count}: Length={enumerator.Current.Length}");
            }
            Console.WriteLine($"  Total sequences enumerated: {count} (Expected: 2)");

            WriteColored("[PASS] Enumerator test passed", ConsoleColor.Green);
        }
    }
}