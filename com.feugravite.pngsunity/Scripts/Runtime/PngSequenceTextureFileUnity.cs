using UnityEngine;

namespace Blayms.PNGS.Unity
{ 
    public class PngSequenceTextureFileUnity : PngSequenceFileUnity<Texture2D>
    {
        internal override Texture2D CreateSourceFromNativeElement(PngSequenceFile.SequenceElement sequenceElement, out Object subAsset)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, 0, true);
            texture.filterMode = FilterMode.Point;
            texture.alphaIsTransparency = true;
            texture.mipMapBias = 0;
            texture.minimumMipmapLevel = 0;
            texture.LoadImage(sequenceElement.EncodeToPNG(), true);
            subAsset = null;
            return texture;
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
