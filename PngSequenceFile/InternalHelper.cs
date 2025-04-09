using System;
using System.Collections.Generic;
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
    }
}
