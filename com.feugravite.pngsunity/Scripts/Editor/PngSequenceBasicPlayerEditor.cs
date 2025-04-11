#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using InspectorEditor = UnityEditor.Editor;
namespace Blayms.PNGS.Unity.Editor
{
    [CustomEditor(typeof(PngSequenceBasicPlayer))]
    public class PngSequenceBasicPlayerEditor : InspectorEditor
    {
        private PngSequenceBasicPlayer playerInstance;
        private bool showPlayerInfo;
        private static readonly Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
        public override bool RequiresConstantRepaint()
        {
            return showPlayerInfo;
        }
        public override void OnInspectorGUI()
        {
            if (playerInstance == null)
            {
                playerInstance = (PngSequenceBasicPlayer)target;
            }
            playerInstance.clip = (PngSequenceTextureFileUnity)EditorGUILayout.ObjectField("Clip", playerInstance.clip, typeof(PngSequenceTextureFileUnity), true);
            playerInstance.bootMode = (BootMode)EditorGUILayout.EnumPopup("Boot Mode", playerInstance.bootMode);
            playerInstance.updateMode = (UpdateMode)EditorGUILayout.EnumPopup("Update Mode", playerInstance.updateMode);
            playerInstance.playbackSpeed = EditorGUILayout.Slider("Playback Speed", playerInstance.playbackSpeed, 0, 100);
            playerInstance.target = (RenderTexture)EditorGUILayout.ObjectField("Render Texture", playerInstance.target, typeof(RenderTexture), true);
            EditorGUILayout.BeginHorizontal();
            playerInstance.overrideLoopCount = EditorGUILayout.Toggle("Override Loop Count", playerInstance.overrideLoopCount);
            if (playerInstance.overrideLoopCount)
            {
                float overrideLoopCount = EditorGUILayout.FloatField("", playerInstance.OverrideLoopCountValue);
                playerInstance.OverrideLoopCountValue = (int)Mathf.Max(overrideLoopCount, -1);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
            if(GUILayout.Button("Align Render Texture"))
            {
                playerInstance.AlignRenderTexture();
            }
            EditorGUILayout.Space(5);
            showPlayerInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showPlayerInfo, "Debug Information");
            if (showPlayerInfo)
            {
                EditorGUI.BeginDisabledGroup(true);
                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUI.DrawRect(rect, bgColor);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.FloatField($"Current Element Index", playerInstance.i_PlaybackJob.currentSequenceIndex, GUILayout.Width(200));
                if (playerInstance.clip != null)
                {
                    float realElementSpeed = playerInstance.clip.sequenceElements[playerInstance.i_PlaybackJob.currentSequenceIndex].lengthSeconds / playerInstance.playbackSpeed;
                    GUILayout.Label($"{FormatTime(realElementSpeed)}", EditorStyles.boldLabel);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                float playbackPos = playerInstance.playbackPositionSeconds;
                EditorGUILayout.FloatField($"Current Position", playbackPos, GUILayout.Width(200));
                GUILayout.Label($"{TimeWord(playbackPos)}", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.FloatField($"Player looped", playerInstance.loopAmount, GUILayout.Width(200));
                GUILayout.Label("times", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Toggle("Player is active?", playerInstance.isPlaying);
                EditorGUILayout.Toggle("Player is paused?", playerInstance.isPaused);

                EditorGUILayout.EndVertical();
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        private static string FormatTime(float seconds)
        {
            if (seconds == Mathf.Infinity)
            {
                return "(∞ seconds)";
            }
            return seconds == 1 ? "(1 second)" : $"({seconds:F2} seconds)";
        }
        private static string TimeWord(float seconds)
        {
            return seconds == 1 ? "second" : "seconds";
        }
    }
}
#endif