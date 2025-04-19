using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Blayms.PNGS.Unity
{
    public sealed class PngSequenceSpriteFileUnity : PngSequenceFileUnity<Sprite>
    {
        internal override Sprite CreateSourceFromNativeElement(PngSequenceFile.SequenceElement sequenceElement, out Object subAsset)
        {
            Texture2D texture = PngSequenceTextureFileUnity.CreateTextureFromNativeElement(sequenceElement);

            float rectX = 0;
            float rectY = 0;
            float rectWidth = texture.width;
            float rectHeight = texture.height;
            float pivotX = 0.5f;
            float pivotY = 0.5f;
            float pixelsPerUnit = 100.0f;

            string metadata = sequenceElement.File.Header.GetMetadataWhere(x => x.StartsWith("unityspriteconfig=")).FirstOrDefault();
            if (!string.IsNullOrEmpty(metadata))
            {
                string[] splits = metadata.Replace("unityspriteconfig=", "").Split(';');

                splits[2] = splits[2].Replace("$texW", texture.width.ToString()).Replace("$texH", texture.height.ToString());
                splits[3] = splits[3].Replace("$texW", texture.width.ToString()).Replace("$texH", texture.height.ToString());

                rectX = float.Parse(splits[0], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                rectY = float.Parse(splits[1], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                rectWidth = float.Parse(splits[2], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                rectHeight = float.Parse(splits[3], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                pivotX = float.Parse(splits[4], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                pivotY = float.Parse(splits[5], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
                pixelsPerUnit = float.Parse(splits[6], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            }

            Sprite sprite = Sprite.Create(texture, new Rect(rectX, rectY, rectWidth, rectHeight), new Vector2(pivotX, pivotY), pixelsPerUnit);
            subAsset = texture;
            return sprite;
        }
        public override void ChangeAssetName(string name)
        {
            base.ChangeAssetName(name);
            for (int i = 0; i < sequenceElements.Length; i++)
            {
                sequenceElements[i].source.texture.name = $"{name}_Texture2D#{i}";
            }
        }
        /// <summary>
        /// Creates an instance of <see cref="PngSequenceSpriteFileUnity"/> from *.pngs file path
        /// </summary>
        public static PngSequenceSpriteFileUnity LoadFromPath(string pngsPath)
        {
            return LoadFromBytes(System.IO.File.ReadAllBytes(pngsPath));
        }
        /// <summary>
        /// Creates an instance of <see cref="PngSequenceSpriteFileUnity"/> from *.pngs file bytes
        /// </summary>
        public static PngSequenceSpriteFileUnity LoadFromBytes(byte[] pngsBytes)
        {
            PngSequenceFile nativeFile = new PngSequenceFile(pngsBytes);
            PngSequenceSpriteFileUnity pngsSprite = InternalHelper.CreateFromRawData<PngSequenceSpriteFileUnity, Sprite>(nativeFile, out Object[] output, out Object[] potentialSubAssets);
            return pngsSprite;
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
