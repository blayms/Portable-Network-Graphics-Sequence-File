using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blayms.PNGS
{
    public class PngSequenceFileWriter : IDisposable
    {
        private readonly BinaryWriter _writer;

        public PngSequenceFileWriter(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            _writer = new BinaryWriter(stream);
        }

        public void Write(PngSequenceFile pngs)
        {
            _writer.Write(Encoding.ASCII.GetBytes(PngSequenceFile.FileHeader.Signature));
            PngParser.WriteBigEndianUInt32(_writer, pngs.Header.IHDR.Width);
            PngParser.WriteBigEndianUInt32(_writer, pngs.Header.IHDR.Height);
            _writer.Write(pngs.Header.IHDR.BitDepth);
            _writer.Write((byte)pngs.Header.IHDR.ColorType);
            _writer.Write((byte)pngs.Header.IHDR.CompressionMethod);
            _writer.Write((byte)pngs.Header.IHDR.FilterMethod);
            _writer.Write((byte)pngs.Header.IHDR.InterlaceMethod);

            IEnumerator<PngSequenceFile.Sequence> enumerator = pngs.GetEnumerator();
            while (enumerator.MoveNext())
            {
                WriteSequence(enumerator.Current);
            }
        }

        private void WriteSequence(PngSequenceFile.Sequence sequence)
        {
            _writer.Write(Encoding.ASCII.GetBytes(PngSequenceFile.Sequence.Signature));
            _writer.Write(sequence.PixelsCount);
            _writer.Write(sequence.Length);
            _writer.Write(sequence.Pixels);
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
