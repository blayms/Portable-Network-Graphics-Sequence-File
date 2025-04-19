using System;
using UnityEngine;

namespace Blayms.PNGS.Unity
{
    [AddComponentMenu("Feugravite/PNG Sequence Sprite Player")]
    public sealed class PngSequenceSpritePlayer : PngSequencePlayerBase<SpriteRenderer, Sprite>
    {
        protected override void PerformFrame()
        {
            if (target != null)
            {
                target.sprite = clip.sequenceElements[i_PlaybackJob.currentSequenceIndex].source;
            }
        }
    }
}
