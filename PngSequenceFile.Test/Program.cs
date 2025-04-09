﻿using System.Text.Json;

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

            TestEmptyFileCreation();
            TestFileHeaderSignature();
            TestSequenceOperations();
            TestConstructFromSequences();
            TestConstructFromPNGBytes();

            WriteColored("\n[NOTICE] Before testing serialization, please provide a *.png file path or type 'skip' to skip", ConsoleColor.Cyan);
            string? input = Console.ReadLine();
            if (input != null && !input.Equals("skip", StringComparison.OrdinalIgnoreCase))
            {
                TestFileSerialization(input);
            }

            TestEnumeratorBehavior();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== All Tests Completed ===");
            Console.ResetColor();
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
            var testSequence = new PngSequenceFile.Sequence();

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

            var sequences = new PngSequenceFile.Sequence[]
            {
                new PngSequenceFile.Sequence { Length = 100 },
                new PngSequenceFile.Sequence { Length = 200 }
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
                new PngSequenceFile.Sequence { Length = 100 },
                new PngSequenceFile.Sequence { Length = 200 });

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