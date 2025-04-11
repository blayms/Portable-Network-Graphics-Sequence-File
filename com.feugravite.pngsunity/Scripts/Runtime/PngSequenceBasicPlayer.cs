using System;
using UnityEngine;

namespace Blayms.PNGS.Unity
{
    public sealed class PngSequenceBasicPlayer : PngSequencePlayerBase<RenderTexture, Texture2D>
    {
        protected override void PerformFrame()
        {
            if (target != null && target.isReadable)
            {
                RenderTexture.active = target;
                Graphics.Blit(clip.sequenceElements[i_PlaybackJob.currentSequenceIndex].source, target);
                RenderTexture.active = null;
            }
        }
        [ContextMenu("Align Render Texture")]
        public void AlignRenderTexture()
        {
            if (clip == null && target == null)
            {
                Debug.LogError($"{gameObject.name} ({nameof(PngSequenceBasicPlayer)}): Both clip and render texture are null. Cannot proceed with alignment!");
                return;
            }
            if (clip == null)
            {
                Debug.LogError($"{gameObject.name} ({nameof(PngSequenceBasicPlayer)}): Clip is null. Cannot proceed with alignment!");
                return;
            }
            if (target == null)
            {
                Debug.LogError($"{gameObject.name} ({nameof(PngSequenceBasicPlayer)}): Render texture is null. Cannot proceed with alignment!");
                return;
            }
            if (!target.isReadable)
            {
                Debug.LogError($"{gameObject.name} ({nameof(PngSequenceBasicPlayer)}): Render Texture is not readable. Cannot proceed with alignment!");
                return;
            }
            target.Release();
            target.width = clip.preferredResolution.x;
            target.height = clip.preferredResolution.y;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }
    }
}
