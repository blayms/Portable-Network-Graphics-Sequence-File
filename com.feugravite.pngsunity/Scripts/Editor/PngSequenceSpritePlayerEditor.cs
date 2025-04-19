#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using InspectorEditor = UnityEditor.Editor;
namespace Blayms.PNGS.Unity.Editor
{
    [CustomEditor(typeof(PngSequenceSpritePlayer))]
    public class PngSequenceSpritePlayerEditor : InspectorEditor
    {
        private PngSequenceSpritePlayer playerInstance;
        private bool showPlayerInfo;
        private static readonly Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
        private SerializedProperty clip;
        private SerializedProperty bootMode;
        private SerializedProperty updateMode;
        private SerializedProperty playbackSpeed;
        private SerializedProperty targetProp;
        private SerializedProperty overrideLoopCount;
        private SerializedProperty overrideLoopCountValue;
        public override bool RequiresConstantRepaint()
        {
            return showPlayerInfo;
        }
        public override void OnInspectorGUI()
        {
            if (clip == null)
            {
                playerInstance = (PngSequenceSpritePlayer)target;
                clip = serializedObject.FindProperty(nameof(PngSequenceBasicPlayer.clip));
                bootMode = serializedObject.FindProperty(nameof(PngSequenceBasicPlayer.bootMode));
                updateMode = serializedObject.FindProperty(nameof(PngSequenceBasicPlayer.updateMode));
                playbackSpeed = serializedObject.FindProperty("m_PlaybackSpeed");
                targetProp = serializedObject.FindProperty(nameof(PngSequenceBasicPlayer.target));
                overrideLoopCount = serializedObject.FindProperty(nameof(PngSequenceBasicPlayer.overrideLoopCount));
                overrideLoopCountValue = serializedObject.FindProperty("m_OverrideValue");
            }
            EditorGUILayout.ObjectField(clip, typeof(PngSequenceSpriteFileUnity), new GUIContent("Clip"));
            EditorGUILayout.PropertyField(bootMode, new GUIContent("Boot Mode"));
            EditorGUILayout.PropertyField(updateMode, new GUIContent("Update Mode"));
            EditorGUILayout.Slider(playbackSpeed, -500f, 500f, new GUIContent("Playback Speed"));
            EditorGUILayout.ObjectField(targetProp, typeof(SpriteRenderer), new GUIContent("Sprite Renderer"));

            if (targetProp.objectReferenceValue == null)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.HelpBox("Sprite Renderer is not assinged and the player will not work! Do you want to create one?", MessageType.Warning);

                if (GUILayout.Button("Create"))
                {
                    SpriteRenderer spriteRenderer = playerInstance.GetComponent<SpriteRenderer>();
                    if (spriteRenderer == null)
                    {
                        spriteRenderer = playerInstance.gameObject.AddComponent<SpriteRenderer>();
                        Undo.RegisterCreatedObjectUndo(spriteRenderer, "Add SpriteRenderer");
                    }
                    targetProp.objectReferenceValue = spriteRenderer;
                    EditorUtility.SetDirty(playerInstance);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            bool currentValue = overrideLoopCount.boolValue;
            EditorGUI.BeginChangeCheck();
            bool newValue = EditorGUILayout.Toggle("Override Loop Count", currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                overrideLoopCount.boolValue = newValue;
            }
            if (currentValue)
            {
                EditorGUI.BeginChangeCheck();
                
                int overrideLoopCountNew = EditorGUILayout.IntField(GUIContent.none, overrideLoopCountValue.intValue);

                if (EditorGUI.EndChangeCheck())
                {
                    overrideLoopCountValue.intValue = Mathf.Max(overrideLoopCountNew, -1);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (serializedObject.ApplyModifiedProperties())
            {
                foreach (var target in targets)
                {
                    EditorUtility.SetDirty(target);
                }
            }

            EditorGUILayout.Space(5);
            showPlayerInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showPlayerInfo, "Debug Information");
            if (showPlayerInfo)
            {
                EditorGUI.BeginDisabledGroup(true);
                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUI.DrawRect(rect, bgColor);
                GUILayout.Space(5);
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

                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                GUIContent playOrStop = !playerInstance.isPlaying ? new GUIContent("\u25BA", "Start playing") : new GUIContent("\u25A0", "Stop playing");
                GUIContent pauseOrUnpause = playerInstance.isPaused ? new GUIContent("\u25B7", "Unpause playback") : new GUIContent("||", "Pause playback");
                if (GUILayout.Button(playOrStop, GUILayout.Width(60)))
                {
                    if (playerInstance.isPlaying)
                    {
                        playerInstance.Stop();
                    }
                    else
                    {
                        playerInstance.Play();
                    }
                }
                if (GUILayout.Button(pauseOrUnpause, GUILayout.Width(60)))
                {
                    playerInstance.Pause(!playerInstance.isPaused);
                }
                if (GUILayout.Button(new GUIContent("\u21BA", "Reset playback"), GUILayout.Width(60)))
                {
                    playerInstance.ResetPlayback(false, playerInstance.playbackSpeed < 0);
                }
                if (GUILayout.Button(new GUIContent("\u25C4", "Reverse playback"), GUILayout.Width(60)))
                {
                    playerInstance.ReversePlayback();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        private static string FormatTime(float seconds)
        {
            bool signed = seconds < 0;
            if (Mathf.Abs(seconds) == Mathf.Infinity)
            {
                return $"({(signed ? "-" : "")}∞ seconds)";
            }
            return Mathf.Abs(seconds) == 1 ? $"({(signed ? "-" : "")}1 second)" : $"({seconds:F2} seconds)";
        }
        private static string TimeWord(float seconds)
        {
            return seconds == 1 ? "second" : "seconds";
        }
    }
}
#endif