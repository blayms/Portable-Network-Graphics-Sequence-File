using UnityEngine;

namespace Blayms.PNGS.Unity
{ 
    public sealed class PngSequenceSpriteFileUnity : PngSequenceFileUnity<Sprite>
    {
        internal override Sprite CreateSourceFromNativeElement(PngSequenceFile.SequenceElement sequenceElement, out Object subAsset)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, 0, true);
            texture.filterMode = FilterMode.Point;
            texture.alphaIsTransparency = true;
            texture.mipMapBias = 0;
            texture.minimumMipmapLevel = 0;
            texture.LoadImage(sequenceElement.EncodeToPNG(), true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            subAsset = texture;
            return sprite;
        }

        public class BasicSpriteSequenceElement : SequenceElement<Sprite>
        {
            public override int pixelsCount => source.texture.width * source.texture.height;
            public override byte[] EncodeToPNG()
            {
                return source.texture.EncodeToPNG();
            }
        }
    }
}
