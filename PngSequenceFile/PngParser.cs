using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace Blayms.PNGS
{
    internal static class PngParser
    {

        public static (IHDRHeader Header, byte[] PixelData) Read(byte[] pngBytes)
        {
            using (MemoryStream stream = new MemoryStream(pngBytes))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {

                    byte[] signature = reader.ReadBytes(8);
                    if (!CompareBytes(signature, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
                        throw new InvalidDataException("Invalid PNG signature");

                    IHDRHeader header = default;
                    List<byte> pixelData = new List<byte>();

                    while (true)
                    {
                        uint chunkLength = ReadBigEndianUInt32(reader);
                        string chunkType = Encoding.ASCII.GetString(reader.ReadBytes(4));
                        byte[] chunkData = reader.ReadBytes((int)chunkLength);
                        uint _ = ReadBigEndianUInt32(reader);

                        if (chunkType == "IHDR")
                        {
                            using (MemoryStream chunkStream = new MemoryStream(chunkData))
                            {
                                using (BinaryReader chunkReader = new BinaryReader(chunkStream))
                                {
                                    header.Width = ReadBigEndianUInt32(chunkReader);
                                    header.Height = ReadBigEndianUInt32(chunkReader);
                                    header.BitDepth = chunkReader.ReadByte();
                                    header.ColorType = (PngColorType)chunkReader.ReadByte();
                                    header.CompressionMethod = (PngCompressionMethod)chunkReader.ReadByte();
                                    header.FilterMethod = (PngFilterMode)chunkReader.ReadByte();
                                    header.InterlaceMethod = (PngInterlaceMethod)chunkReader.ReadByte();
                                }
                            }
                        }
                        else if (chunkType == "IDAT")
                        {
                            pixelData.AddRange(chunkData);
                        }
                        else if (chunkType == "IEND")
                        {
                            break;
                        }
                    }

                    return (header, pixelData.ToArray());
                }
            }
        }

        public static byte[] Write(IHDRHeader header, byte[] pixelData)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

            byte[] ihdrData = new byte[13];
            using (MemoryStream ms = new MemoryStream(ihdrData))
            {
                using (BinaryWriter chunkWriter = new BinaryWriter(ms))
                {
                    WriteBigEndianUInt32(chunkWriter, header.Width);
                    WriteBigEndianUInt32(chunkWriter, header.Height);
                    chunkWriter.Write(header.BitDepth);
                    chunkWriter.Write((byte)header.ColorType);
                    chunkWriter.Write((byte)header.CompressionMethod);
                    chunkWriter.Write((byte)header.FilterMethod);
                    chunkWriter.Write((byte)header.InterlaceMethod);
                }
            }
            WriteChunk(writer, "IHDR", ihdrData);
            WriteChunk(writer, "IDAT", pixelData);
            WriteChunk(writer, "IEND", Array.Empty<byte>());

            return stream.ToArray();
        }

        private static void WriteChunk(BinaryWriter writer, string chunkType, byte[] chunkData)
        {
            WriteBigEndianUInt32(writer, (uint)chunkData.Length);

            byte[] typeBytes = Encoding.ASCII.GetBytes(chunkType);
            writer.Write(typeBytes);
            writer.Write(chunkData);

            byte[] crcInput = new byte[typeBytes.Length + chunkData.Length];
            Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
            Buffer.BlockCopy(chunkData, 0, crcInput, typeBytes.Length, chunkData.Length);
            uint crc = Crc32.Compute(crcInput);
            WriteBigEndianUInt32(writer, crc);
        }

        internal static uint ReadBigEndianUInt32(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }
        internal static int ReadBigEndianInt32(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        internal static void WriteBigEndianUInt32(BinaryWriter writer, uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            writer.Write(bytes);
        }
        internal static void WriteBigEndianInt32(BinaryWriter writer, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            writer.Write(bytes);
        }

        private static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
    internal static class Crc32
    {
        private static readonly uint[] Table;
        private const uint Polynomial = 0xedb88320;

        static Crc32()
        {
            Table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                    crc = (crc >> 1) ^ ((crc & 1) * Polynomial);
                Table[i] = crc;
            }
        }

        public static uint Compute(byte[] data)
        {
            uint crc = 0xffffffff;
            foreach (byte b in data)
                crc = (crc >> 8) ^ Table[(crc ^ b) & 0xff];
            return crc ^ 0xffffffff;
        }
    }
}