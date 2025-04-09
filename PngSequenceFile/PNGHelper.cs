using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;

namespace Blayms.PNGS
{
    public static class PNGHelper
    {
        public struct IHDRChunk
        {
            public uint Width;
            public uint Height;
            public byte BitDepth;
            public byte ColorType;
            public byte CompressionMethod;
            public byte FilterMethod;
            public byte InterlaceMethod;
        }
        public static (IHDRChunk, byte[] pixelData) Read(byte[] pngData, bool decompressPixels = true)
        {
            using (MemoryStream ms = new MemoryStream(pngData))
            {
                // Read the PNG signature (8 bytes)
                byte[] signature = new byte[8];
                ms.Read(signature, 0, 8);

                if (BitConverter.ToString(signature) != "89-50-4E-47-0D-0A-1A-0A")
                {
                    throw new Exception("Invalid PNG signature.");
                }

                Console.WriteLine("PNG Signature validated.");

                // Read the PNG chunks
                List<IHDRChunk> ihdrChunks = new List<IHDRChunk>();
                List<byte[]> imageDataChunks = new List<byte[]>();
                uint width = 0, height = 0;

                while (ms.Position < ms.Length)
                {
                    // Read the length of the next chunk (4 bytes)
                    byte[] lengthBytes = new byte[4];
                    ms.Read(lengthBytes, 0, 4);
                    uint chunkLength = BitConverter.ToUInt32(lengthBytes, 0);

                    // Read the chunk type (4 bytes)
                    byte[] chunkTypeBytes = new byte[4];
                    ms.Read(chunkTypeBytes, 0, 4);
                    string chunkType = Encoding.ASCII.GetString(chunkTypeBytes);

                    // Read the chunk data
                    byte[] chunkData = new byte[chunkLength];
                    ms.Read(chunkData, 0, (int)chunkLength);

                    // Handle the chunk based on its type
                    if (chunkType == "IHDR")
                    {
                        IHDRChunk ihdr = ParseIHDR(chunkData);
                        ihdrChunks.Add(ihdr);
                        width = ihdr.Width;
                        height = ihdr.Height;
                        Console.WriteLine($"IHDR: Width={width}, Height={height}, BitDepth={ihdr.BitDepth}, ColorType={ihdr.ColorType}");
                    }
                    else if (chunkType == "IDAT")
                    {
                        // Store IDAT (compressed image data)
                        imageDataChunks.Add(chunkData);
                        Console.WriteLine("Found IDAT chunk (image data).");
                    }
                    else if (chunkType == "IEND")
                    {
                        // End of PNG file
                        Console.WriteLine("End of PNG file.");
                        break;
                    }
                    else
                    {
                        // Skip unknown chunks (and their CRC)
                        ms.Read(new byte[4], 0, 4); // CRC bytes
                    }
                }

                byte[] pixelData = null;
                if (imageDataChunks.Count > 0)
                {
                    byte[] allCompressedData = CombineChunks(imageDataChunks);

                    if (decompressPixels)
                    {
                        byte[] decompressedData = DecompressIdatData(allCompressedData);
                        Console.WriteLine($"Decompressed Image Data Length: {decompressedData.Length}");

                        // The decompressed data is raw pixel data (with filters applied)
                        // At this point, you would need to process the filter methods and extract raw pixel data.
                        pixelData = ProcessRawPixelData(decompressedData, width, height);
                    }
                    else
                    {
                        // If not decompressing, just return the compressed data for the IDAT chunks.
                        pixelData = allCompressedData;
                    }
                }

                // Return the IHDR chunk and the pixel data (either raw or compressed)
                IHDRChunk header = ihdrChunks.Count > 0 ? ihdrChunks[0] : default;
                return (header, pixelData);
            }
        }

        // Helper function to parse IHDR chunk
        private static IHDRChunk ParseIHDR(byte[] data)
        {
            IHDRChunk chunk = new IHDRChunk
            {
                Width = BitConverter.ToUInt32(data, 0),
                Height = BitConverter.ToUInt32(data, 4),
                BitDepth = data[8],
                ColorType = data[9],
                CompressionMethod = data[10],
                FilterMethod = data[11],
                InterlaceMethod = data[12]
            };
            return chunk;
        }

        // Helper function to combine all IDAT chunks into one byte array
        private static byte[] CombineChunks(List<byte[]> chunks)
        {
            int totalLength = 0;
            foreach (var chunk in chunks)
            {
                totalLength += chunk.Length;
            }

            byte[] combinedData = new byte[totalLength];
            int offset = 0;

            foreach (var chunk in chunks)
            {
                Buffer.BlockCopy(chunk, 0, combinedData, offset, chunk.Length);
                offset += chunk.Length;
            }

            return combinedData;
        }

        // Helper function to decompress IDAT data (DEFLATE)
        private static byte[] DecompressIdatData(byte[] compressedData)
        {
            using (MemoryStream ms = new MemoryStream(compressedData))
            using (DeflateStream deflateStream = new DeflateStream(ms, CompressionMode.Decompress))
            using (MemoryStream decompressedStream = new MemoryStream())
            {
                deflateStream.CopyTo(decompressedStream);
                return decompressedStream.ToArray();
            }
        }

        // Example function to process raw pixel data (after decompressing)
        // You would need to handle PNG filters and color types here for full fidelity
        private static byte[] ProcessRawPixelData(byte[] decompressedData, uint width, uint height)
        {
            // Assuming RGBA format for simplicity (you can modify this for different color types)
            int pixelCount = (int)(width * height);
            byte[] pixelData = new byte[pixelCount * 4]; // 4 bytes per pixel (RGBA)

            // For simplicity, just copy the data (in real scenario, handle filtering)
            Buffer.BlockCopy(decompressedData, 0, pixelData, 0, decompressedData.Length);

            return pixelData;
        }
    }
}
