using System.Linq;
using UnityEngine;

namespace Blayms.PNGS.Unity
{
    public class PngSequenceTextureFileUnity : PngSequenceFileUnity<Texture2D>
    {
        internal override Texture2D CreateSourceFromNativeElement(PngSequenceFile.SequenceElement sequenceElement, out Object subAsset)
        {
            subAsset = null;
            return CreateTextureFromNativeElement(sequenceElement);
        }
        internal static Texture2D CreateTextureFromNativeElement(PngSequenceFile.SequenceElement sequenceElement)
        {
            try
            {
                TextureFormat textureFormat = TextureFormat.RGBA32;
                int mipCount = 0;
                bool linear = false;
                FilterMode filterMode = FilterMode.Point;
                int mipMapBias = 0;
                int minimumMipmapLevel = 0;
                int anisoLevel = 1;
                bool isReadable = false;
                TextureWrapMode wrapMode = TextureWrapMode.Clamp;

                string metadata = sequenceElement.File.Header.GetMetadataWhere(x => x.StartsWith("unitytexconfig=")).FirstOrDefault();
                if (!string.IsNullOrEmpty(metadata))
                {
                    string[] splits = metadata.Replace("unitytexconfig=", "").Split(';');
                    textureFormat = System.Enum.Parse<TextureFormat>(splits[0]);
                    mipCount = int.Parse(splits[1]);
                    linear = bool.Parse(splits[2]);
                    filterMode = System.Enum.Parse<FilterMode>(splits[3]);
                    mipMapBias = int.Parse(splits[4]);
                    minimumMipmapLevel = int.Parse(splits[5]);
                    anisoLevel = int.Parse(splits[6]);
                    isReadable = bool.Parse(splits[7]);
                    wrapMode = System.Enum.Parse<TextureWrapMode>(splits[8]);
                }

                Texture2D texture = new Texture2D(2, 2, textureFormat, mipCount, linear);
                texture.filterMode = filterMode;
                texture.mipMapBias = mipMapBias;
                texture.minimumMipmapLevel = minimumMipmapLevel;
                texture.anisoLevel = anisoLevel;
                texture.wrapMode = wrapMode;
#if UNITY_EDITOR
                texture.alphaIsTransparency = true;
#endif
                texture.LoadImage(sequenceElement.EncodeToPNG(), isReadable);
                return texture;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// Creates an instance of <see cref="PngSequenceTextureFileUnity"/> from *.pngs file path
        /// </summary>
        public static PngSequenceTextureFileUnity LoadFromPath(string pngsPath)
        {
            return LoadFromBytes(System.IO.File.ReadAllBytes(pngsPath));
        }
        /// <summary>
        /// Creates an instance of <see cref="PngSequenceTextureFileUnity"/> from *.pngs file bytes
        /// </summary>
        public static PngSequenceTextureFileUnity LoadFromBytes(byte[] pngsBytes)
        {
            PngSequenceFile nativeFile = new PngSequenceFile(pngsBytes);
            PngSequenceTextureFileUnity pngsTex = InternalHelper.CreateFromRawData<PngSequenceTextureFileUnity, Texture2D>(nativeFile, out Object[] output, out Object[] potentialSubAssets);
            return pngsTex;
        }
        public class BasicTexture2DSequenceElement : SequenceElement<Texture2D>
        {
            public override int pixelsCount => source.width * source.height;
            public override byte[] EncodeToPNG()
            {
                return source.EncodeToPNG();
            }
        }
    }
}
