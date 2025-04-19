using UnityEngine;

namespace Blayms.PNGS.Unity
{
    public class PngSequenceFileUnity<TSource> : ScriptableObject where TSource : Object
    {
        internal PngSequenceFile native;
        [field: SerializeField] public SequenceElement<TSource>[] sequenceElements { get; internal set; }
        [field: SerializeField] public int loopCount { get; internal set; } = -1;
        /// <summary>
        /// The preferred resolution for rendering this PNG Sequence file
        /// </summary>
        [field: SerializeField] public Vector2Int preferredResolution { get; internal set; }
        /// <summary>
        /// Total duration / length of the PNG Sequence file
        /// </summary>
        public uint totalLength
        {
            get
            {
                uint ms = 0;
                for (int i = 0; i < sequenceElements.Length; i++)
                {
                    ms += sequenceElements[i].length;
                }
                return ms;
            }
        }
        /// <summary>
        /// Highly recommended method to change a name of this PNGS file object, instead of using a "name" property
        /// </summary>
        /// <param name="name"></param>
        public virtual void ChangeAssetName(string name)
        {
            this.name = name;
            for (int i = 0; i < sequenceElements.Length; i++)
            {
                sequenceElements[i].source.name = $"{name}_{typeof(TSource).Name}#{i}";
            }
        }
        internal virtual TSource CreateSourceFromNativeElement(PngSequenceFile.SequenceElement sequenceElement, out Object subAsset)
        {
            subAsset = null;
            return default;
        }
        [System.Obsolete("This function is highly unstable and not guaranteed to return the native PngSequenceFile object", false)]
        public PngSequenceFile GetNativeObject()
        {
            return native;
        }
    }
}
