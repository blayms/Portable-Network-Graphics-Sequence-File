using Krashan.LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Blayms.PNGS
{
    /// <summary>
    /// Represent a writer for <see cref="PngSequenceFile"/>
    /// </summary>
    public class PngSequenceFileWriter : IDisposable
    {
        private readonly BinaryWriter _writer;
        private const CompressionLevel compressionLevel = CompressionLevel.Optimal;

        public PngSequenceFileWriter(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            _writer = new BinaryWriter(stream);
        }
        /// <summary>
        /// Writes a specific <see cref="PngSequenceFile"/>
        /// </summary>
        public void Write(PngSequenceFile pngs)
        {
            _writer.Write(Encoding.ASCII.GetBytes(PngSequenceFile.FileHeader.Signature));

            _writer.Write(pngs.Header.Version);

            PngParser.WriteBigEndianUInt32(_writer, pngs.Header.IHDR.Width);
            PngParser.WriteBigEndianUInt32(_writer, pngs.Header.IHDR.Height);
            _writer.Write(pngs.Header.LoopCount);
            _writer.Write(pngs.Header.IHDR.BitDepth);
            _writer.Write((byte)pngs.Header.IHDR.ColorType);
            _writer.Write((byte)pngs.Header.IHDR.CompressionMethod);
            _writer.Write((byte)pngs.Header.IHDR.FilterMethod);
            _writer.Write((byte)pngs.Header.IHDR.InterlaceMethod);

            _writer.Write(Encoding.ASCII.GetBytes(PngSequenceFile.FileHeader.MetadataSignature));
            byte[][] metadataEncodedEntries = new byte[pngs.Header.GetMetadataCount()][];
            IEnumerator<string> metadataEntries = pngs.Header.GetMetadataEnumerator();
            int currentMetadata = 0;
            while (metadataEntries.MoveNext())
            {
                metadataEncodedEntries[currentMetadata] = Encoding.ASCII.GetBytes(metadataEntries.Current);
                currentMetadata++;
            }
            _writer.Write((uint)pngs.Header.GetMetadataCount());
            for (int i = 0; i < metadataEncodedEntries.Length; i++)
            {
                _writer.Write((uint)metadataEncodedEntries[i].Length);
                _writer.Write(metadataEncodedEntries[i]);
            }

            IEnumerator<PngSequenceFile.SequenceElement> enumerator = pngs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                WriteSequence(enumerator.Current);
            }
        }

        private void WriteSequence(PngSequenceFile.SequenceElement sequence)
        {
            _writer.Write(Encoding.ASCII.GetBytes(PngSequenceFile.SequenceElement.Signature));
            byte[] compressedPixels = CompressData(sequence.Pixels);
            _writer.Write((uint)compressedPixels.Length);

            _writer.Write(sequence.Length);
            _writer.Write(compressedPixels);
        }
        internal static byte[] CompressData(byte[] input)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (LZ4Stream compressor = new LZ4Stream(output, CompressionMode.Compress, LZ4StreamFlags.HighCompression))
                {
                    compressor.Write(input, 0, input.Length);
                }
                return output.ToArray();
            }
        }

        internal static byte[] DecompressData(byte[] compressedInput, int originalLength = 0)
        {
            using (MemoryStream input = new MemoryStream(compressedInput))
            {
                using (LZ4Stream decompressor = new LZ4Stream(input, CompressionMode.Decompress, LZ4StreamFlags.HighCompression))
                {
                    using (MemoryStream output = new MemoryStream(compressedInput.Length * 2))
                    {
                        decompressor.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
        }
        private bool _disposed = false;
        public void Dispose()
        {
            if (!_disposed)
            {
                _writer.Dispose();
                _disposed = true;
            }
        }
    }
}
