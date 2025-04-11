using System;
using UnityEngine;

namespace Blayms.PNGS.Unity
{
    public class PngSequenceSpritePlayer : PngSequencePlayerBase<SpriteRenderer, Sprite>
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
