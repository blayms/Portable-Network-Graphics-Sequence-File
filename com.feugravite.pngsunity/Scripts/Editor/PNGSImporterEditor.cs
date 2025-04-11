#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
namespace Blayms.PNGS.Unity.Editor
{
    [CustomEditor(typeof(PNGSImporter))]
    public class PNGSImporterEditor : ScriptedImporterEditor
    {
        private PngSequenceTextureFileUnity pngTexture;
        private PngSequenceSpriteFileUnity pngSprite;
        private bool initialized;
        private float totalLength = -1;
        private float previewPlaybackSpeed = 1;
        private int previewPreviewMsPosition;
        private int previewAnimationCurrentIndex;
        private string loopCount = null;
        private bool previewAvailable = false;
        private double lastUpdateTime;
        public override bool HasPreviewGUI()
        {
            return previewAvailable;
        }
        public override bool RequiresConstantRepaint()
        {
            return previewAvailable;
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void OnPreviewSettings()
        {
            EditorGUILayout.BeginHorizontal();
            float newPreviewPlaybackSpeed = GUILayout.HorizontalSlider(previewPlaybackSpeed, 0f, 100f, GUILayout.Width(100));

            if (!Mathf.Approximately(newPreviewPlaybackSpeed, previewPlaybackSpeed))
            {
                previewPlaybackSpeed = Event.current.shift ? Mathf.Round(newPreviewPlaybackSpeed) : newPreviewPlaybackSpeed;
            }

            GUILayout.Label(previewPlaybackSpeed < 100 ? $"{previewPlaybackSpeed:F2}%" : "100,0%", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }
        public override void DrawPreview(Rect previewArea)
        {
            double now = EditorApplication.timeSinceStartup;
            float deltaTime = Mathf.Clamp((float)(now - lastUpdateTime), 0.0001f, 0.1f);
            lastUpdateTime = now;

            if (pngTexture != null)
            {
                if (pngTexture.sequenceElements[previewAnimationCurrentIndex].source != null)
                {
                    if (previewPlaybackSpeed > 0)
                    {
                        int millisecondsToAdd = Mathf.RoundToInt(deltaTime * 1000 * previewPlaybackSpeed);
                        previewPreviewMsPosition += millisecondsToAdd;
                        uint totalLength = pngTexture.totalLength;
                        if (previewPreviewMsPosition > totalLength)
                        {
                            previewPreviewMsPosition = 0;
                            previewAnimationCurrentIndex = 0;
                        }
                        int totalFrames = pngTexture.sequenceElements.Length;
                        int newIndex = (int)(previewPreviewMsPosition * totalFrames / totalLength);
                        newIndex = Mathf.Clamp(newIndex, 0, totalFrames - 1);
                        previewAnimationCurrentIndex = newIndex;
                    }

                    Texture2D texture = pngTexture.sequenceElements[previewAnimationCurrentIndex].source;
                    float scale = Mathf.Min(
                        previewArea.width / texture.width,
                        previewArea.height / texture.height
                    );
                    Rect textureRect = new Rect(
                        previewArea.x + (previewArea.width - texture.width * scale) * 0.5f,
                        previewArea.y + (previewArea.height - texture.height * scale) * 0.5f,
                        texture.width * scale,
                        texture.height * scale
                    );
                    GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.DropShadowLabel(previewArea, $"No texture found for sequence element #{previewAnimationCurrentIndex}!");
                }
            }
            if (pngSprite != null)
            {
                if (pngSprite.sequenceElements[previewAnimationCurrentIndex].source != null)
                {
                    if (previewPlaybackSpeed > 0)
                    {
                        int millisecondsToAdd = Mathf.RoundToInt(deltaTime * 1000 * previewPlaybackSpeed);
                        previewPreviewMsPosition += millisecondsToAdd;
                        uint totalLength = pngSprite.totalLength;
                        if (previewPreviewMsPosition > totalLength)
                        {
                            previewPreviewMsPosition = 0;
                            previewAnimationCurrentIndex = 0;
                        }
                        int totalFrames = pngSprite.sequenceElements.Length;
                        int newIndex = (int)(previewPreviewMsPosition * totalFrames / totalLength);
                        newIndex = Mathf.Clamp(newIndex, 0, totalFrames - 1);
                        previewAnimationCurrentIndex = newIndex;
                    }

                    Texture2D texture = pngSprite.sequenceElements[previewAnimationCurrentIndex].source.texture;
                    float scale = Mathf.Min(
                        previewArea.width / texture.width,
                        previewArea.height / texture.height
                    );
                    Rect textureRect = new Rect(
                        previewArea.x + (previewArea.width - texture.width * scale) * 0.5f,
                        previewArea.y + (previewArea.height - texture.height * scale) * 0.5f,
                        texture.width * scale,
                        texture.height * scale
                    );
                    GUI.DrawTexture(textureRect, texture, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.DropShadowLabel(previewArea, $"No texture found for sequence element #{previewAnimationCurrentIndex}!");
                }
            }
        }
        public override void OnInspectorGUI()
        {
            if (!initialized)
            {
                string assetPath = ((AssetImporter)target).assetPath;
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                string typeName = obj.GetType().Name;
                switch (typeName)
                {
                    case nameof(PngSequenceTextureFileUnity):
                        pngTexture = (PngSequenceTextureFileUnity)obj;
                        previewAvailable = true;
                        break;
                    case nameof(PngSequenceSpriteFileUnity):
                        pngSprite = (PngSequenceSpriteFileUnity)obj;
                        previewAvailable = true;
                        break;
                }
                initialized = true;
            }

            if (pngTexture != null)
            {
                if (totalLength == -1)
                {
                    totalLength = (float)System.Math.Round(pngTexture.sequenceElements.Sum(x => x.length) / 1000f, 2);
                }
                if (loopCount == null)
                {
                    loopCount = (pngTexture.loopCount < 0) ? "Infinity" : pngTexture.loopCount.ToString();
                }
                int ogFontSize = EditorStyles.boldLabel.fontSize;
                EditorStyles.boldLabel.fontSize *= 2;
                GUILayout.Label("Portable Network Graphics\nSequence File by Blayms", EditorStyles.boldLabel);
                EditorStyles.boldLabel.fontSize = ogFontSize;
                GUILayout.Label($"\nType: Texture2Ds", EditorStyles.boldLabel);
                GUILayout.Label($"\nFile name: {pngTexture.name}", EditorStyles.boldLabel);
                GUILayout.Label($"Sequence Elements Count: {pngTexture.sequenceElements.Length}", EditorStyles.boldLabel);
                GUILayout.Label($"Total duration: {totalLength} seconds", EditorStyles.boldLabel);
                GUILayout.Label($"Loop count: {loopCount}", EditorStyles.boldLabel);
                GUILayout.Label($"Preferred Resolution: {pngTexture.preferredResolution.x}x{pngTexture.preferredResolution.y}", EditorStyles.boldLabel);
            }

            if (pngSprite != null)
            {
                if (totalLength == -1)
                {
                    totalLength = (float)System.Math.Round(pngSprite.sequenceElements.Sum(x => x.length) / 1000f, 2);
                }
                if (loopCount == null)
                {
                    loopCount = (pngSprite.loopCount < 0) ? "Infinity" : pngSprite.loopCount.ToString();
                }
                int ogFontSize = EditorStyles.boldLabel.fontSize;
                EditorStyles.boldLabel.fontSize *= 2;
                GUILayout.Label("Portable Network Graphics\nSequence File by Blayms", EditorStyles.boldLabel);
                EditorStyles.boldLabel.fontSize = ogFontSize;
                GUILayout.Label($"\nType: Sprites", EditorStyles.boldLabel);
                GUILayout.Label($"\nFile name: {pngSprite.name}", EditorStyles.boldLabel);
                GUILayout.Label($"Sequence Elements Count: {pngSprite.sequenceElements.Length}", EditorStyles.boldLabel);
                GUILayout.Label($"Total duration: {totalLength} seconds", EditorStyles.boldLabel);
                GUILayout.Label($"Loop count: {loopCount}", EditorStyles.boldLabel);
                GUILayout.Label($"Preferred Resolution: {pngSprite.preferredResolution.x}x{pngSprite.preferredResolution.y}", EditorStyles.boldLabel);
            }
        }
        public override bool showImportedObject => false;
        protected override bool needsApplyRevert => false;
    }
}
#endif
