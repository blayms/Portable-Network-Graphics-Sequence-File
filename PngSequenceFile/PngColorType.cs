namespace Blayms.PNGS
{
    /// <summary>
    /// Supported PNG color types
    /// </summary>
    public enum PngColorType : byte
    {
        /// <summary>Grayscale (1, 2, 4, 8, or 16 bits per pixel)</summary>
        Grayscale = 0,
        /// <summary>Truecolor (8 or 16 bits per sample)</summary>
        Truecolor = 2,
        /// <summary>Indexed-color (1, 2, 4, or 8 bits per pixel)</summary>
        Indexed = 3,
        /// <summary>Grayscale with alpha (8 or 16 bits per sample)</summary>
        GrayscaleAlpha = 4,
        /// <summary>Truecolor with alpha (8 or 16 bits per sample)</summary>
        TruecolorAlpha = 6
    }
}
