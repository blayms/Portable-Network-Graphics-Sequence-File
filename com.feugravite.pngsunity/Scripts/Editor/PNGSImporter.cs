#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Blayms.PNGS.Unity.Editor
{
    [ScriptedImporter(1, "pngs", AllowCaching = true)]
    public class PNGSImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            PngSequenceFile pngsFile = new PngSequenceFile(File.ReadAllBytes(ctx.assetPath));

            IEnumerable<string> possibleMetadata = pngsFile.Header.GetMetadataWhere(x => x.StartsWith("unityType"));

            string sourceType = typeof(Texture2D).Name;

            if (possibleMetadata.Any())
            {
                string preservedType = possibleMetadata.First().Split('=')[1].ToLower();
                switch (preservedType)
                {
                    case "sprite":
                        sourceType = typeof(Sprite).Name;
                        break;
                    case "texture2d":
                        sourceType = typeof(Texture2D).Name;
                        break;
                }
            }
            Object[] objectsToSub = new Object[0];
            Object[] objectsToSubToSub = new Object[0];
            Object createdSeq = null;
            switch (sourceType)
            {
                case nameof(Texture2D):
                    createdSeq = InternalHelper.CreateFromRawData<PngSequenceTextureFileUnity, Texture2D>(pngsFile, out objectsToSub, out objectsToSubToSub);
                    break;
                case nameof(Sprite):
                    createdSeq = InternalHelper.CreateFromRawData<PngSequenceSpriteFileUnity, Sprite>(pngsFile, out objectsToSub, out objectsToSubToSub);
                    break;
            }

            ctx.AddObjectToAsset("PngSequenceFile", createdSeq);
            ctx.SetMainObject(createdSeq);
            for (int i = 0; i < objectsToSubToSub.Length; i++)
            {
                if (objectsToSubToSub[i] != null)
                {
                    objectsToSubToSub[i].name = $"Texture_{Path.GetFileNameWithoutExtension(ctx.assetPath)}_{i}";
                    ctx.AddObjectToAsset($"Texture_{i}", objectsToSubToSub[i]);
                    objectsToSubToSub[i].hideFlags = HideFlags.HideInHierarchy;
                }
            }
            for (int i = 0; i < objectsToSub.Length; i++)
            {
                if (objectsToSub[i] != null)
                {
                    objectsToSub[i].name = $"{Path.GetFileNameWithoutExtension(ctx.assetPath)}_{i}";
                    ctx.AddObjectToAsset($"Sprite_{i}", objectsToSub[i]);
                }
            }

            TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>(ctx.assetPath);
            if (file != null)
            {
                file.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }
            EditorUtility.SetDirty(createdSeq);
        }
    }
}
#endif