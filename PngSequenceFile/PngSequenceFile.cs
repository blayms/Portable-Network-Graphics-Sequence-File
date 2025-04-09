using System;
using System.IO;

namespace PngSequenceFile
{
    public class PngSequenceFile
    {
        public FileHeader Header { get; internal set; }
        public Sequence[] Sequences { get; internal set; }
        public PngSequenceFile(string filePath) : this(File.ReadAllBytes(filePath))
        {
        }
        public PngSequenceFile(byte[] fileBytes)
        {

        }
        public class FileHeader
        {
            public const string Signature = "$PNGS";
            public const int FrameCount = 0;
            public const short Size = 21;
        }
        public class Sequence
        {
            public byte[] Pixels { get; internal set; }
            public int Length { get; internal set; }

            public Sequence(string pngPath, int msLength)
            {
                Length = msLength;
            }
        }
    }
}
