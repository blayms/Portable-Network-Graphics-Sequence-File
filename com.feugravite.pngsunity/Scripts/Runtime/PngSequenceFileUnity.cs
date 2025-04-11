using UnityEngine;

namespace Blayms.PNGS.Unity
{
    public class PngSequenceFileUnity<TSource> : ScriptableObject where TSource : Object
    {
        internal PngSequenceFile native;
        [field: SerializeField] public SequenceElement<TSource>[] sequenceElements { get; internal set; }
        [field: SerializeField] public int loopCount { get; internal set; } = -1;
        [field: SerializeField] public Vector2Int preferredResolution { get; internal set; }
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
        internal virtual TSource CreateSourceFromNativeElement(PngSequenceFile.SequenceElement sequenceElement, out Object subAsset)
        {
            subAsset = null;
            return default;
        }
        
        public PngSequenceFile GetNativeObject()
        {
            return native;
        }
    }
}
