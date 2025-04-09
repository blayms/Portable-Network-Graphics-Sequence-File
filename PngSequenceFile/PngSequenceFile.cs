using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Blayms.PNGS.PngSequenceFile;

namespace Blayms.PNGS
{
    /// <summary>
    /// Represents a *.pngs file format made by Blayms
    /// </summary>
    [Serializable]
    public class PngSequenceFile : IEnumerable<SequenceElement>
    {
        /// <summary>
        /// <inheritdoc cref="FileHeader"/>
        /// </summary>
        public FileHeader Header { get; internal set; }
        internal List<SequenceElement> Sequences { get; set; } = new List<SequenceElement>();
        /// <summary>
        /// An amount of sequence elememts
        /// </summary>
        public int Count => Sequences.Count;
        /// <summary>
        /// Creates an instance of <see cref="PngSequenceFile"/> from file path
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <exception cref="Exceptions.PNGSReadFailedException">Signature check failed</exception>
        public PngSequenceFile(string filePath) : this(File.ReadAllBytes(filePath))
        {
        }
        /// <summary>
        /// Creates an instance of <see cref="PngSequenceFile"/> from file bytes
        /// </summary>
        /// <param name="fileBytes">File bytes</param>
        /// <exception cref="Exceptions.PNGSReadFailedException">Signature check failed</exception>
        public PngSequenceFile(byte[] fileBytes)
        {
            using (MemoryStream ms = new MemoryStream(fileBytes))
            {
                using (BinaryReader binaryReader = new BinaryReader(ms))
                {
                    if (Encoding.ASCII.GetString(binaryReader.ReadBytes(5)) != FileHeader.Signature)
                    {
                        throw new Exceptions.PNGSReadFailedException($"no {FileHeader.Signature} signature was found at byte #{ms.Position}!");
                    }
                    uint width = PngParser.ReadBigEndianUInt32(binaryReader);
                    uint height = PngParser.ReadBigEndianUInt32(binaryReader);
                    int loopCount = PngParser.ReadBigEndianInt32(binaryReader);
                    byte bitDepth = binaryReader.ReadByte();
                    PngColorType colorType = (PngColorType)binaryReader.ReadByte();
                    PngCompressionMethod compressionMethod = (PngCompressionMethod)binaryReader.ReadByte();
                    PngFilterMode filterMethod = (PngFilterMode)binaryReader.ReadByte();
                    PngInterlaceMethod interlaceMethod = (PngInterlaceMethod)binaryReader.ReadByte();

                    Header = new FileHeader();
                    Header.File = this;
                    Header.LoopCount = loopCount;
                    Header.IHDR = new IHDRHeader(width, height, bitDepth, colorType, compressionMethod, filterMethod, interlaceMethod);

                    while (ms.Position < ms.Length)
                    {
                        if (Encoding.ASCII.GetString(binaryReader.ReadBytes(4)) != SequenceElement.Signature)
                        {
                            throw new Exceptions.PNGSReadFailedException($"no {SequenceElement.Signature} signature was found at byte #{ms.Position}!");
                        }
                        int pixelsCount = binaryReader.ReadInt32();
                        int seqLength = binaryReader.ReadInt32();
                        byte[] pixels = binaryReader.ReadBytes(pixelsCount);

                        SequenceElement sequence = new SequenceElement();
                        sequence.Length = seqLength;
                        sequence.Pixels = pixels;
                        sequence.File = this;
                        AddSequence(sequence);
                    }
                }
            }
        }
        /// <summary>
        /// Get a sequence element by index
        /// </summary>
        public SequenceElement this[int index]
        {
            get
            {
                return Sequences[index];
            }
            set
            {
                Sequences[index] = value;
            }
        }
        public PngSequenceFile()
        {

        }
        /// <summary>
        /// Creates an instance of <see cref="PngSequenceFile"/> from directly from sequence elements and builds it's header depending on the least or most valuable sequence element. 
        /// </summary>
        /// <param name="preferMaximizedValues">Defines if header should be constructed from the least or the most valuable sequence element in the collection</param>
        /// <param name="sequences">Sequences</param>
        public static PngSequenceFile ConstructFromSequences(bool preferMaximizedValues, params SequenceElement[] sequences)
        {
            PngSequenceFile pngs = new PngSequenceFile();
            pngs.Sequences = sequences.ToList();
            pngs.Header = new FileHeader(pngs, preferMaximizedValues);
            for (int i = 0; i < sequences.Length; i++)
            {
                sequences[i].File = pngs;
            }
            return pngs;
        }
        /// <summary>
        /// Creates an instance of <see cref="PngSequenceFile"/> from directly from *.png bytes with equal duration<br>for each sequence element and builds it's header depending on the least or most valuable sequence element.</br>
        /// </summary>
        /// <param name="preferMaximizedValues">Defines if header should be constructed from the least or the most valuable sequence element in the collection</param>
        /// <param name="msDuration">Defines how many milliseconds each sequence element will last</param>
        /// <param name="pngBytes">*.png bytes</param>
        public static PngSequenceFile ConstructFromPNGWithEqualDuration(bool preferMaximizedValues, int msDuration, params byte[][] pngBytes)
        {
            PngSequenceFile pngs = new PngSequenceFile();
            for (int i = 0; i < pngBytes.Length; i++)
            {
                pngs.AddSequence(new SequenceElement(pngBytes[i], msDuration));
            }
            pngs.Header = new FileHeader(pngs, preferMaximizedValues);
            for (int i = 0; i < pngs.Sequences.Count; i++)
            {
                pngs.Sequences[i].File = pngs;
            }
            return pngs;
        }
        /// <summary>
        /// Creates an instance of <see cref="PngSequenceFile"/> from directly from *.png file paths with equal duration<br>for each sequence element and builds it's header depending on the least or most valuable sequence element.</br>
        /// </summary>
        /// <param name="preferMaximizedValues">Defines if header should be constructed from the least or the most valuable sequence element in the collection</param>
        /// <param name="msDuration">Defines how many milliseconds each sequence element will last</param>
        /// <param name="pngFiles">*.png file paths</param>
        public static PngSequenceFile ConstructFromPNGWithEqualDuration(bool preferMaximizedValues, int msDuration, params string[] pngFiles)
        {
            return ConstructFromPNGWithEqualDuration(preferMaximizedValues, msDuration, InternalHelper.PathsToBytes(pngFiles));
        }
        /// <summary>
        /// Adds a <see cref="SequenceElement"/> to the file
        /// </summary>
        public void AddSequence(SequenceElement sequence)
        {
            if (!Sequences.Contains(sequence))
            {
                Sequences.Add(sequence);
            }
        }
        /// <summary>
        /// Insert a <see cref="SequenceElement"/> to the file
        /// </summary>
        public void InsertSequence(int index, SequenceElement sequence)
        {
            if (!Sequences.Contains(sequence))
            {
                Sequences.Insert(index, sequence);
            }
        }
        /// <summary>
        /// Does it contain a <see cref="SequenceElement"/> in the file?
        /// </summary>
        public bool ContainsSequence(SequenceElement sequence)
        {
            return Sequences.Contains(sequence);
        }
        /// <summary>
        /// Adds a range of <see cref="SequenceElement"/> instances to the file
        /// </summary>
        public void AddRangeOfSequences(ICollection<SequenceElement> sequences)
        {
            IEnumerator<SequenceElement> enumerator = sequences.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AddSequence(enumerator.Current);
            }
        }
        /// <summary>
        /// <inheritdoc cref="AddRangeOfSequences(ICollection{SequenceElement})"/>
        /// </summary>
        public void AddRangeOfSequences(params SequenceElement[] sequences)
        {
            for (int i = 0; i < sequences.Length; i++)
            {
                AddSequence(sequences[i]);
            }
        }
        /// <summary>
        /// Removes a <see cref="SequenceElement"/> from the file
        /// </summary>
        public void RemoveSequence(SequenceElement sequence)
        {
            if (Sequences.Contains(sequence))
            {
                Sequences.Remove(sequence);
            }
        }
        /// <summary>
        /// Removes a <see cref="SequenceElement"/> at the certain index from the file
        /// </summary>
        public void RemoveSequenceAt(int index)
        {
            if (Sequences.Count >= index)
            {
                Sequences.RemoveAt(index);
            }
        }
        /// <summary>
        /// <para><b>Usage example:</b></para>
        /// <code>
        /// // Load a PNG sequence file
        /// var pngFile = new PngSequenceFile("animation.png");
        /// 
        /// // Iterate through sequences using foreach
        /// foreach (var sequenceEl in pngFile)
        /// {
        ///     Console.WriteLine($"SequenceElement with {sequenceEl.Pixels.Length} pixels");
        /// }
        /// 
        /// // Alternative: Manual enumeration
        /// using (var enumerator = pngFile.GetEnumerator())
        /// {
        ///     while (enumerator.MoveNext())
        ///     {
        ///         var sequence = enumerator.Current;
        ///         // Process sequence...
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the sequences.</returns>
        /// <remarks>
        /// The enumerator will return sequences in the order they appear in the file.
        /// Dispose the enumerator when manual enumeration is complete.
        /// </remarks>
        public IEnumerator<SequenceElement> GetEnumerator()
        {
            return Sequences.GetEnumerator();
        }
        /// <summary>
        /// <inheritdoc cref="GetEnumerator"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /// <summary>
        /// Represents a *.pngs file header
        /// </summary>

        [Serializable]
        public class FileHeader
        {
            public const string Signature = "$PNGS";
            public const short Size = 22;
            public int LoopCount = -1;
            /// <summary>
            /// <inheritdoc cref="PngSequenceFile"/>
            /// </summary>
            public PngSequenceFile File { get; internal set; }
            /// <summary>
            /// <inheritdoc cref="IHDRHeader"/>
            /// </summary>
            public IHDRHeader IHDR { get; set; }
            public FileHeader()
            {

            }
            internal FileHeader(PngSequenceFile file, bool preferMaximized)
            {
                File = file;
                Refresh(preferMaximized);
            }
            internal void Refresh(bool preferMaximized)
            {
                IHDRHeader[] ihdrHeaders = File.Sequences.Select(x => x.ihdrChunk).ToArray();
                IHDR = new IHDRHeader
                (
                    width: preferMaximized ? ihdrHeaders.Max(x => x.Width) : ihdrHeaders.Min(x => x.Width),
                    height: preferMaximized ? ihdrHeaders.Max(x => x.Height) : ihdrHeaders.Min(x => x.Height),
                    bitDepth: preferMaximized ? ihdrHeaders.Max(x => x.BitDepth) : ihdrHeaders.Min(x => x.BitDepth),
                    colorType: preferMaximized ? ihdrHeaders.Max(x => x.ColorType) : ihdrHeaders.Min(x => x.ColorType),
                    compressionMethod: preferMaximized ? ihdrHeaders.Max(x => x.CompressionMethod) : ihdrHeaders.Min(x => x.CompressionMethod),
                    filterMethod: preferMaximized ? ihdrHeaders.Max(x => x.FilterMethod) : ihdrHeaders.Min(x => x.FilterMethod),
                    interlaceMethod: preferMaximized ? ihdrHeaders.Max(x => x.InterlaceMethod) : ihdrHeaders.Min(x => x.InterlaceMethod)
                );
            }
        }
        /// <summary>
        /// Represents a block of data that contains pixels and frame duration
        /// </summary>
        [Serializable]
        public class SequenceElement
        {
            internal IHDRHeader ihdrChunk;
            public const string Signature = "$SEQ";
            public PngSequenceFile File { get; internal set; }
            /// <summary>
            /// Pixel data of *.png file in a 1D array
            /// </summary>
            public byte[] Pixels { get; internal set; }
            /// <summary>
            /// Duration / Length of the sequence in milliseconds (ms)
            /// </summary>
            public int Length { get; internal set; }
            /// <summary>
            /// An amount of pixels from <see cref="Pixels"/>
            /// </summary>
            public int PixelsCount => Pixels.Length;
            public SequenceElement()
            {

            }
            /// <summary>
            /// Converts this sequence element to *.png bytes
            /// </summary>
            /// <returns></returns>
            public byte[] EncodeToPNG()
            {
                return PngParser.Write(File.Header.IHDR, Pixels);
            }
            /// <summary>
            /// Creates an instance of <see cref="SequenceElement"/> from *.png file path duration in milliseconds (ms)
            /// </summary>
            /// <param name="pngPath">*.png file path</param>
            /// <param name="msLength">Duration in milliseconds (ms)</param>
            public SequenceElement(string pngPath, int msLength) : this(System.IO.File.ReadAllBytes(pngPath), msLength)
            {

            }
            /// <summary>
            /// Creates an instance of <see cref="SequenceElement"/> from *.png file bytes duration in milliseconds (ms)
            /// </summary>
            /// <param name="pngBytes">*.png file bytes</param>
            /// <param name="msLength">Duration in milliseconds (ms)</param>
            public SequenceElement(byte[] pngBytes, int msLength)
            {
                SwapPng(pngBytes);
                Length = msLength;
            }
            /// <summary>
            /// Changes the image data of this sequence element from bytes
            /// </summary>
            public void SwapPng(byte[] pngData)
            {
                (IHDRHeader, byte[]) png = PngParser.Read(pngData);
                ihdrChunk = png.Item1;
                Pixels = png.Item2;
            }
            /// <summary>
            /// Changes the image data of this sequence element from file path
            /// </summary>
            public void SwapPng(string pngBytes)
            {
                SwapPng(System.IO.File.ReadAllBytes(pngBytes));
            }
        }
    }
}
