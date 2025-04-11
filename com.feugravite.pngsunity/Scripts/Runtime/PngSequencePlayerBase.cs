using System;
using UnityEngine;

namespace Blayms.PNGS.Unity
{
    public class PngSequencePlayerBase<TTarget, TSource> : MonoBehaviour where TTarget : UnityEngine.Object where TSource : UnityEngine.Object
    {
        [SerializeField] private PngSequenceFileUnity<TSource> m_Clip;
        public BootMode bootMode = BootMode.None;
        public UpdateMode updateMode = UpdateMode.Scaled;
        [SerializeField][Range(0f, 100f)] private float m_PlaybackSpeed = 1;
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
        public PngSequenceFileUnity<TSource> clip
        {
            get => m_Clip;
            set
            {
                m_Clip = value;
            }
        }
        public float playbackSpeed
        {
            get => m_PlaybackSpeed;
            set
            {
                m_PlaybackSpeed = Mathf.Clamp(value, 0f, 100f);
            }
        }
        public int focusedElement => i_PlaybackJob.currentSequenceIndex;
        public int OverrideLoopCountValue
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
        public void ResetPlayback(bool resetLoopCount)
        {
            i_PlaybackJob.currentPositionMS = 0;
            i_PlaybackJob.currentSequenceIndex = 0;
            if (resetLoopCount)
            {
                i_PlaybackJob.loopAmount = 0;
            }
        }
        public void Play(PngSequenceFileUnity<TSource> clip = null)
        {
            if (clip != null)
            {
                m_Clip = clip;
            }
            if (m_Clip == null)
            {
                Debug.LogWarning("The clip is null, can't play the sequence!");
                return;
            }
            i_PlaybackJob.isActive = true;
            i_PlaybackJob.isPaused = false;
            onStarted?.Invoke(m_Clip);
        }
        public void Stop()
        {
            ResetPlayback(true);
            onStoppedManually?.Invoke(clip);
            i_PlaybackJob.isActive = false;
        }
        public void Pause(bool value)
        {
            (value ? onPaused : onUnpaused)?.Invoke(m_Clip);
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
        private void Update()
        {
            if (isPlaying)
            {
                float deltaTime = updateMode == UpdateMode.Scaled ? Time.deltaTime : Time.unscaledDeltaTime;
                int millisecondsToAdd = Mathf.RoundToInt(deltaTime * 1000 * m_PlaybackSpeed);
                i_PlaybackJob.currentPositionMS += millisecondsToAdd;
                uint totalLength = clip.totalLength;
                if (i_PlaybackJob.currentPositionMS > totalLength)
                {
                    i_PlaybackJob.loopAmount++;
                    ResetPlayback(false);
                    onLoopPointReached?.Invoke(m_Clip);
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

                onNewFrame?.Invoke(m_Clip);
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
