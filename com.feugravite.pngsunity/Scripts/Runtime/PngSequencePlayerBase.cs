using System;
using UnityEngine;

namespace Blayms.PNGS.Unity
{
    public class PngSequencePlayerBase<TTarget, TSource> : MonoBehaviour where TTarget : UnityEngine.Object where TSource : UnityEngine.Object
    {
        public PngSequenceFileUnity<TSource> clip;
        public BootMode bootMode = BootMode.None;
        public UpdateMode updateMode = UpdateMode.Scaled;
        [SerializeField][Range(-500f, 500f)] private float m_PlaybackSpeed = 1;
        public TTarget target;
        [Space(10)] public bool overrideLoopCount;
        [SerializeField][Min(-1)] private int m_OverrideValue = -1;

        internal PNGSPlaybackJob i_PlaybackJob;

        public delegate void PlaybackJobDelegate(PngSequenceFileUnity<TSource> clip);
        public event PlaybackJobDelegate onLoopPointReached;
        public event PlaybackJobDelegate onStarted;
        public event PlaybackJobDelegate onPaused;
        public event PlaybackJobDelegate onUnpaused;
        public event PlaybackJobDelegate onStoppedManually;
        public event PlaybackJobDelegate onNewFrame;

        public bool isPlaying => i_PlaybackJob.isActive && !i_PlaybackJob.isPaused && clip != null;
        public bool isPaused => i_PlaybackJob.isPaused;
        public int loopAmount => i_PlaybackJob.loopAmount;
        /// <summary>
        /// Playback position in milliseconds
        /// </summary>
        [property: SerializeField]
        public int playbackPositionMs
        {
            get
            {
                return i_PlaybackJob.currentPositionMS;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                i_PlaybackJob.currentPositionMS = value;
            }
        }
        /// <summary>
        /// Playback position in seconds
        /// </summary>
        [property: SerializeField]
        public float playbackPositionSeconds
        {
            get
            {
                return i_PlaybackJob.currentPositionMS / 1000f;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                i_PlaybackJob.currentPositionMS = (int)(value * 1000);
            }
        }
        /// <summary>
        /// Speed of the playback
        /// </summary>
        [property: SerializeField]
        public float playbackSpeed
        {
            get => m_PlaybackSpeed;
            set
            {
                m_PlaybackSpeed = Mathf.Clamp(value, -500f, 500f);
            }
        }
        /// <summary>
        /// Current sequence element index
        /// </summary>
        public int focusedElement => i_PlaybackJob.currentSequenceIndex;
        /// <summary>
        /// Overrides the loop count. To apply process of overriding the loop count, see <see cref="overrideLoopCount"/>
        /// </summary>
        [property: SerializeField]
        public int overrideLoopCountValue
        {
            get => m_OverrideValue;
            set
            {
                if (value < -1)
                {
                    value = -1;
                }
                m_OverrideValue = value;
            }
        }
        private void Awake()
        {
            if (bootMode == BootMode.Awake)
            {
                Play();
            }
        }
        private void Start()
        {
            if (bootMode == BootMode.Start)
            {
                Play();
            }
        }
        /// <summary>
        /// Reverses playback by flipping it's speed value sign
        /// </summary>
        public void ReversePlayback(bool value)
        {
            float abs = Mathf.Abs(playbackSpeed);
            playbackSpeed = value ? -abs : abs;
        }
        /// <summary>
        /// Reverses playback by flipping it's speed value sign
        /// </summary>
        public void ReversePlayback()
        {
            playbackSpeed = -playbackSpeed;
        }
        /// <summary>
        /// Resets the playback to either start or end
        /// </summary>
        public void ResetPlayback(bool resetLoopCount, bool tryStartingFromEnd)
        {
            if (!tryStartingFromEnd)
            {
                i_PlaybackJob.currentPositionMS = 0;
                i_PlaybackJob.currentSequenceIndex = 0;
            }
            else
            {
                i_PlaybackJob.currentPositionMS = (int)clip.totalLength;
                i_PlaybackJob.currentSequenceIndex = clip.sequenceElements.Length - 1;
            }
            if (resetLoopCount)
            {
                i_PlaybackJob.loopAmount = 0;
            }
        }
        /// <summary>
        /// Initiatiates the playback from the begining
        /// </summary>
        public void Play(PngSequenceFileUnity<TSource> clip = null)
        {
            if (clip != null)
            {
                this.clip = clip;
            }
            if (this.clip == null)
            {
                Debug.LogWarning("The clip is null, can't play the sequence!");
                return;
            }
            ResetPlayback(false, playbackSpeed < 0);
            i_PlaybackJob.isActive = true;
            i_PlaybackJob.isPaused = false;
            onStarted?.Invoke(clip);
        }
        /// <summary>
        /// Stops the playback
        /// </summary>
        public void Stop()
        {
            ResetPlayback(true, false);
            onStoppedManually?.Invoke(clip);
            i_PlaybackJob.isActive = false;
        }
        /// <summary>
        /// Pauses / unpauses the playback
        /// </summary>
        public void Pause(bool value)
        {
            (value ? onPaused : onUnpaused)?.Invoke(clip);
            i_PlaybackJob.isPaused = value;
        }
        private int ActualMaxLoopCount()
        {
            if (overrideLoopCount)
            {
                return m_OverrideValue;
            }
            else
            {
                if (clip != null)
                {
                    return clip.loopCount;
                }
                else
                {
                    return -1;
                }
            }
        }
        /// <summary>
        /// Advances the playback by milliseconds. Negative argument can be used to wind back
        /// </summary>
        public void Advance(int milliseconds)
        {
            if (playbackSpeed != 0) // If playback speed is 0, calculation is useless, so we need to prevent it!
            {
                i_PlaybackJob.currentPositionMS += milliseconds;
                uint totalLength = clip.totalLength;
                if (i_PlaybackJob.currentPositionMS > totalLength || i_PlaybackJob.currentPositionMS < 0)
                {
                    i_PlaybackJob.loopAmount++;
                    ResetPlayback(false, i_PlaybackJob.currentPositionMS < 0);
                    onLoopPointReached?.Invoke(clip);
                }
                int actualMaxLoopCount = ActualMaxLoopCount();
                if (actualMaxLoopCount > -1)
                {
                    if (i_PlaybackJob.loopAmount >= actualMaxLoopCount)
                    {
                        Stop();
                    }
                }

                int totalFrames = clip.sequenceElements.Length;
                int newIndex = (int)(i_PlaybackJob.currentPositionMS * totalFrames / totalLength);
                newIndex = Mathf.Clamp(newIndex, 0, totalFrames - 1);
                i_PlaybackJob.currentSequenceIndex = newIndex;
                PerformFrame();

                onNewFrame?.Invoke(clip);
            }
        }
        private void Update()
        {
            if (isPlaying)
            {
                float deltaTime = updateMode == UpdateMode.Scaled ? Time.deltaTime : Time.unscaledDeltaTime;
                int millisecondsToAdd = Mathf.RoundToInt(deltaTime * 1000 * m_PlaybackSpeed);
                Advance(millisecondsToAdd);
            }
        }
        protected virtual void PerformFrame()
        {

        }
        [Serializable]
        internal struct PNGSPlaybackJob
        {
            public int currentSequenceIndex;
            public int currentPositionMS;
            public int loopAmount;
            public bool isActive;
            public bool isPaused;
        }
    }
}
