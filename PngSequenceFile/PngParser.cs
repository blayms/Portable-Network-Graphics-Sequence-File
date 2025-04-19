using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Blayms.PNGS
{

    internal static class PngParser
    {
        private static readonly byte[] Sig = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        /// <summary>
        /// Converts any input PNG bytes into a valid Truecolor+Alpha PNG.
        /// </summary>
        public static byte[] ToTruecolorAlpha(byte[] pngBytes)
        {
            // 1) Parse chunks
            IHDRHeader ihdr; byte[] idatComp; byte[] palette = null; byte[] trans = null;
            ParseChunks(pngBytes, out ihdr, out idatComp, out palette, out trans);

            // 2) Decompress IDAT (zlib -> raw scanlines)
            byte[] rawScan = ZlibDecompress(idatComp);

            // 3) Unfilter scanlines
            int bpp = GetBytesPerPixel(ihdr.ColorType, ihdr.BitDepth);
            byte[] pixels = Unfilter(rawScan, (int)ihdr.Width, (int)ihdr.Height, bpp);

            // 4) Expand to RGBA
            byte[] rgba = ExpandToRGBA(pixels, ihdr, palette, trans);

            // 5) Re-filter (we'll use None)
            byte[] scanOut = FilterNone(rgba, (int)ihdr.Width, (int)ihdr.Height, 4);

            // 6) Recompress to zlib
            byte[] outComp = ZlibCompress(scanOut);

            // 7) Build new IHDR
            ihdr.ColorType = PngColorType.TruecolorAlpha;
            ihdr.BitDepth = 8;
            ihdr.CompressionMethod = PngCompressionMethod.Deflate;
            ihdr.FilterMethod = PngFilterMode.Adaptive;
            ihdr.InterlaceMethod = PngInterlaceMethod.None;

            // 8) Write final PNG
            return Write(ihdr, outComp);
        }
        /// <summary>
        /// Converts any input PNG bytes into a valid TruecolorAlpha PNG and returns both the IHDR header and the new PNG data.
        /// </summary>
        public static (IHDRHeader Header, byte[] PngData) ToTruecolorAlphaWithHeader(byte[] pngBytes)
        {
            // 1) Parse chunks
            IHDRHeader ihdr;
            byte[] idatComp, palette, trans;
            ParseChunks(pngBytes, out ihdr, out idatComp, out palette, out trans);

            // 2) Decompress IDAT
            byte[] rawScan = ZlibDecompress(idatComp);
            int bppIn = GetBytesPerPixel(ihdr.ColorType, ihdr.BitDepth);
            byte[] pixels = Unfilter(rawScan, (int)ihdr.Width, (int)ihdr.Height, bppIn);

            // 3) Expand to RGBA and re-filter/recompress
            byte[] rgba = ExpandToRGBA(pixels, ihdr, palette, trans);
            byte[] scanOut = FilterNone(rgba, (int)ihdr.Width, (int)ihdr.Height, 4);
            byte[] outComp = ZlibCompress(scanOut);

            // 4) Set new IHDR
            ihdr.ColorType = PngColorType.TruecolorAlpha;
            ihdr.BitDepth = 8;
            ihdr.CompressionMethod = PngCompressionMethod.Deflate;
            ihdr.FilterMethod = PngFilterMode.Adaptive;
            ihdr.InterlaceMethod = PngInterlaceMethod.None;

            // 5) Write final PNG
            byte[] pngOut = Write(ihdr, outComp);
            return (ihdr, pngOut);
        }
        public static (IHDRHeader Header, byte[] CompressedIDAT) ToTruecolorAlphaChunked(byte[] pngBytes)
        {
            IHDRHeader ihdr;
            byte[] idatComp, palette, trans;
            ParseChunks(pngBytes, out ihdr, out idatComp, out palette, out trans);

            byte[] rawScan = ZlibDecompress(idatComp);
            int bppIn = GetBytesPerPixel(ihdr.ColorType, ihdr.BitDepth);
            byte[] pixels = Unfilter(rawScan, (int)ihdr.Width, (int)ihdr.Height, bppIn);
            byte[] rgba = ExpandToRGBA(pixels, ihdr, palette, trans);
            byte[] scanOut = FilterNone(rgba, (int)ihdr.Width, (int)ihdr.Height, 4);
            byte[] outComp = ZlibCompress(scanOut);

            ihdr.ColorType = PngColorType.TruecolorAlpha;
            ihdr.BitDepth = 8;
            ihdr.CompressionMethod = PngCompressionMethod.Deflate;
            ihdr.FilterMethod = PngFilterMode.Adaptive;
            ihdr.InterlaceMethod = PngInterlaceMethod.None;

            return (ihdr, outComp);
        }
        private static void ParseChunks(byte[] data, out IHDRHeader hdr, out byte[] idat, out byte[] plte, out byte[] trns)
        {
            hdr = default; var id = new List<byte>(); plte = null; trns = null;
            using (var ms = new MemoryStream(data)) using (var r = new BinaryReader(ms))
            {
                var sig = r.ReadBytes(8);
                if (!Compare(sig, Sig)) throw new InvalidDataException("Bad PNG signature");

                while (true)
                {
                    uint len = ReadBE(r);
                    string type = Encoding.ASCII.GetString(r.ReadBytes(4));
                    byte[] chunk = r.ReadBytes((int)len);
                    _ = ReadBE(r); // crc
                    if (type == "IHDR")
                    {
                        using (var cm = new MemoryStream(chunk)) using (var cr = new BinaryReader(cm))
                        {
                            hdr.Width = ReadBE(cr); hdr.Height = ReadBE(cr);
                            hdr.BitDepth = cr.ReadByte(); hdr.ColorType = (PngColorType)cr.ReadByte();
                            hdr.CompressionMethod = (PngCompressionMethod)cr.ReadByte(); hdr.FilterMethod = (PngFilterMode)cr.ReadByte();
                            hdr.InterlaceMethod = (PngInterlaceMethod)cr.ReadByte();
                        }
                    }
                    else if (type == "IDAT") { id.AddRange(chunk); }
                    else if (type == "PLTE") { plte = chunk; }
                    else if (type == "tRNS") { trns = chunk; }
                    else if (type == "IEND") break;
                }
            }
            idat = id.ToArray();
        }

        private static byte[] ZlibDecompress(byte[] comp)
        {
            using (var inMs = new MemoryStream(comp))
            {
                // skip zlib header
                inMs.ReadByte(); inMs.ReadByte();
                using (var ds = new DeflateStream(inMs, CompressionMode.Decompress))
                using (var outMs = new MemoryStream())
                {
                    ds.CopyTo(outMs);
                    return outMs.ToArray();
                }
            }
        }

        private static byte[] ZlibCompress(byte[] raw)
        {
            using (var outMs = new MemoryStream())
            {
                // zlib header: CM=8, CINFO=7 => 0x78, default flags=0x9C
                outMs.WriteByte(0x78); outMs.WriteByte(0x9C);
                using (var ds = new DeflateStream(outMs, CompressionLevel.Optimal, true))
                {
                    ds.Write(raw, 0, raw.Length);
                }
                uint adl = Adler32.Compute(raw);
                outMs.Write(new[]{
                    (byte)(adl>>24),(byte)(adl>>16),(byte)(adl>>8),(byte)adl
                }, 0, 4);
                return outMs.ToArray();
            }
        }

        private static int GetBytesPerPixel(PngColorType ct, int bitDepth)
        {
            int comps = 0;
            switch(ct)
            {
                case PngColorType.Grayscale: comps = 1; break;
                case PngColorType.Indexed: comps = 1; break;
                case PngColorType.Truecolor: comps = 3; break;
                case PngColorType.GrayscaleAlpha: comps = 2; break;
                case PngColorType.TruecolorAlpha: comps = 4; break;
                default: comps = 0; break;
            };
            return (comps * bitDepth + 7) / 8;
        }

        private static byte[] Unfilter(byte[] data, int w, int h, int bpp)
        {
            int stride = (bpp * w) + 1;
            byte[] outp = new byte[h * w * bpp];

            for (int y = 0; y < h; y++)
            {
                int inOffset = y * stride;
                int outOffset = y * w * bpp;
                byte filterType = data[inOffset];

                // Copy current scanline
                byte[] curr = new byte[w * bpp];
                Array.Copy(data, inOffset + 1, curr, 0, w * bpp);

                // Copy previous scanline if exists
                byte[] prev = new byte[w * bpp];
                if (y > 0)
                    Array.Copy(outp, (y - 1) * w * bpp, prev, 0, w * bpp);

                ApplyFilter(filterType, curr, prev, bpp);

                // Write filtered data
                Array.Copy(curr, 0, outp, outOffset, w * bpp);
            }

            return outp;
        }

        private static void ApplyFilter(byte filterType, byte[] curr, byte[] prev, int bpp)
        {
            int length = curr.Length;
            switch (filterType)
            {
                case 0: // None
                    return;
                case 1: // Sub
                    for (int i = bpp; i < length; i++)
                        curr[i] = (byte)((curr[i] + curr[i - bpp]) & 0xFF);
                    return;
                case 2: // Up
                    for (int i = 0; i < length; i++)
                        curr[i] = (byte)((curr[i] + prev[i]) & 0xFF);
                    return;
                case 3: // Average
                    for (int i = 0; i < length; i++)
                    {
                        int left = (i >= bpp) ? curr[i - bpp] : 0;
                        int up = prev[i];
                        curr[i] = (byte)((curr[i] + ((left + up) / 2)) & 0xFF);
                    }
                    return;
                case 4: // Paeth
                    for (int i = 0; i < length; i++)
                    {
                        int left = (i >= bpp) ? curr[i - bpp] : 0;
                        int up = prev[i];
                        int pr = left + up - ((left * up) >> 8);
                        int pa = Math.Abs(pr - left);
                        int pb = Math.Abs(pr - up);
                        int pc = Math.Abs(pr - ((left + up) >> 1));
                        int paeth = (pa <= pb && pa <= pc) ? left : (pb <= pc ? up : ((left + up) >> 1));
                        curr[i] = (byte)((curr[i] + paeth) & 0xFF);
                    }
                    return;
                default:
                    throw new InvalidDataException("Unknown PNG filter: " + filterType);
            }
        }

        private static byte[] ExpandToRGBA(byte[] pix, IHDRHeader hdr, byte[] pal, byte[] tr)
        {
            int w = (int)hdr.Width, h = (int)hdr.Height;
            var outp = new byte[w * h * 4];
            int bpp = GetBytesPerPixel(hdr.ColorType, hdr.BitDepth);
            for (int i = 0; i < w * h; i++)
            {
                int inPos = i * bpp;
                int outPos = i * 4;
                switch (hdr.ColorType)
                {
                    case PngColorType.Grayscale:
                        byte g = pix[inPos];
                        outp[outPos] = g; outp[outPos + 1] = g; outp[outPos + 2] = g; outp[outPos + 3] = 255;
                        break;
                    case PngColorType.GrayscaleAlpha:
                        byte gy = pix[inPos], ay = pix[inPos + 1];
                        outp[outPos] = gy; outp[outPos + 1] = gy; outp[outPos + 2] = gy; outp[outPos + 3] = ay;
                        break;
                    case PngColorType.Truecolor:
                        outp[outPos] = pix[inPos]; outp[outPos + 1] = pix[inPos + 1]; outp[outPos + 2] = pix[inPos + 2]; outp[outPos + 3] = 255;
                        break;
                    case PngColorType.TruecolorAlpha:
                        outp[outPos] = pix[inPos]; outp[outPos + 1] = pix[inPos + 1];
                        outp[outPos + 2] = pix[inPos + 2]; outp[outPos + 3] = pix[inPos + 3];
                        break;
                    case PngColorType.Indexed:
                        byte idx = pix[inPos];
                        outp[outPos] = pal[idx * 3];
                        outp[outPos + 1] = pal[idx * 3 + 1];
                        outp[outPos + 2] = pal[idx * 3 + 2];
                        outp[outPos + 3] = tr != null && idx < tr.Length ? tr[idx] : (byte)255;
                        break;
                }
            }
            return outp;
        }

        private static byte[] FilterNone(byte[] data, int w, int h, int comps)
        {
            int stride = (comps * w) + 1;
            var outp = new byte[h * stride];
            for (int y = 0; y < h; y++)
            {
                int inRow = y * w * comps, outRow = y * stride;
                outp[outRow] = 0;
                Buffer.BlockCopy(data, inRow, outp, outRow + 1, w * comps);
            }
            return outp;
        }

        public static byte[] Write(IHDRHeader hdr, byte[] compIDAT)
        {
            using (var ms = new MemoryStream()) using (var w = new BinaryWriter(ms))
            {
                w.Write(Sig);
                // IHDR
                var ih = new byte[13];
                using (var cms = new MemoryStream(ih)) using (var cw = new BinaryWriter(cms))
                {
                    WriteBE(cw, hdr.Width); WriteBE(cw, hdr.Height);
                    cw.Write(hdr.BitDepth); cw.Write((byte)hdr.ColorType);
                    cw.Write((byte)hdr.CompressionMethod); cw.Write((byte)hdr.FilterMethod);
                    cw.Write((byte)hdr.InterlaceMethod);
                }
                WriteChunk(w, "IHDR", ih);
                // IDAT
                WriteChunk(w, "IDAT", compIDAT);
                // IEND
                WriteChunk(w, "IEND", Array.Empty<byte>());
                return ms.ToArray();
            }
        }

        private static void WriteChunk(BinaryWriter w, string t, byte[] d)
        {
            WriteBE(w, (uint)d.Length);
            var bt = Encoding.ASCII.GetBytes(t); w.Write(bt); w.Write(d);
            var buf = new byte[bt.Length + d.Length];
            Buffer.BlockCopy(bt, 0, buf, 0, bt.Length); Buffer.BlockCopy(d, 0, buf, bt.Length, d.Length);
            WriteBE(w, Crc32.Compute(buf));
        }

        private static uint ReadBE(BinaryReader r) { var b = r.ReadBytes(4); if (BitConverter.IsLittleEndian) Array.Reverse(b); return BitConverter.ToUInt32(b, 0); }
        private static void WriteBE(BinaryWriter w, uint v) { var b = BitConverter.GetBytes(v); if (BitConverter.IsLittleEndian) Array.Reverse(b); w.Write(b); }
        private static bool Compare(byte[] a, byte[] b) { if (a.Length != b.Length) return false; for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false; return true; }
    }

    internal static class Crc32
    {
        private const uint Poly = 0xedb88320u;
        private static readonly uint[] Table;
        static Crc32() { Table = new uint[256]; for (uint i = 0; i < 256; i++) { uint c = i; for (int j = 0; j < 8; j++) c = (c >> 1) ^ ((c & 1) * Poly); Table[i] = c; } }
        public static uint Compute(byte[] data) { uint crc = 0xffffffff; foreach (var b in data) crc = (crc >> 8) ^ Table[(crc ^ b) & 0xff]; return crc ^ 0xffffffff; }
    }

    internal static class Adler32
    {
        public static uint Compute(byte[] data)
        {
            const uint MOD = 65521;
            uint a = 1, b = 0;
            foreach (var d in data) { a = (a + d) % MOD; b = (b + a) % MOD; }
            return (b << 16) | a;
        }
    }
}
