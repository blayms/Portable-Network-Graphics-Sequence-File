using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blayms.PNGS
{
    internal static class InternalHelper
    {
        public static byte[][] PathsToBytes(string[] paths)
        {
            List<byte[]> bytes = new List<byte[]>();
            for (int i = 0; i < paths.Length; i++)
            {
                bytes.Add(System.IO.File.ReadAllBytes(paths[i]));
            }
            return bytes.ToArray();
        }

        public static void Swap<T>(this List<T> list, int index1, int index2)
        {
            if (index1 < 0 || index1 >= list.Count || index2 < 0 || index2 >= list.Count)
                throw new ArgumentOutOfRangeException("Index out of range.");

            (list[index1], list[index2]) = (list[index2], list[index1]);
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
    }
}
