using System;

namespace Blayms.PNGS
{
    /// <summary>
    /// Represents the IHDR chunk of a PNG file, containing the image's metadata.
    /// </summary>
    [Serializable]
    public struct IHDRHeader
    {
        /// <summary>
        /// Image width in pixels
        /// </summary>
        public uint Width;

        /// <summary>
        /// Image height in pixels
        /// </summary>
        public uint Height;

        /// <summary>
        /// Bit depth (number of bits per sample or palette index)
        /// </summary>
        public byte BitDepth;

        /// <summary>
        /// Color type of the PNG image
        /// </summary>
        public PngColorType ColorType;

        /// <summary>
        /// Compression method used
        /// </summary>
        public PngCompressionMethod CompressionMethod;

        /// <summary>
        /// Filter method used
        /// </summary>
        public PngFilterMode FilterMethod;

        /// <summary>
        /// Interlace method used
        /// </summary>
        public PngInterlaceMethod InterlaceMethod;

        /// <summary>
        /// Size of the IHDR chunk in bytes
        /// </summary>
        public const short Size = 13;

        /// <summary>
        /// Creates a new IHDR header with the specified parameters
        /// </summary>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="bitDepth">Bit depth (1, 2, 4, 8, or 16)</param>
        /// <param name="colorType">Color type of the PNG image</param>
        /// <param name="compressionMethod">Compression method used</param>
        /// <param name="filterMethod">Filter method used</param>
        /// <param name="interlaceMethod">Interlace method used</param>
        public IHDRHeader(uint width, uint height, byte bitDepth, PngColorType colorType,
                         PngCompressionMethod compressionMethod = PngCompressionMethod.Deflate,
                         PngFilterMode filterMethod = PngFilterMode.Adaptive,
                         PngInterlaceMethod interlaceMethod = PngInterlaceMethod.None)
        {
            Width = width;
            Height = height;
            BitDepth = bitDepth;
            ColorType = colorType;
            CompressionMethod = compressionMethod;
            FilterMethod = filterMethod;
            InterlaceMethod = interlaceMethod;
        }
    }
}