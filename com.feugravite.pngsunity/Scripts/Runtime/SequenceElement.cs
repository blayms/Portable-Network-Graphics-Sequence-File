using System;
using UnityEngine;

namespace Blayms.PNGS.Unity
{
    [Serializable]
    public class SequenceElement<T>
    {
        [field: SerializeField] public T source { get; internal set; }
        [field: SerializeField] public uint length { get; internal set; }
        public virtual int pixelsCount => 0;

        public float lengthSeconds
        {
            get
            {
                return length / 1000f;
            }
        }

        public virtual byte[] EncodeToPNG()
        {
            return null;
        }
    }
}
