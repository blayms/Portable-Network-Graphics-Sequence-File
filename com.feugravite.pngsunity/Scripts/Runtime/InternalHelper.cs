using System.Collections.Generic;
using UnityEngine;

namespace Blayms.PNGS.Unity
{
    internal static class InternalHelper
    {
        public static TFile CreateFromRawData<TFile, TSource>(PngSequenceFile raw, out Object[] outputSources, out Object[] potentialSubAssets) where TSource : Object where TFile : PngSequenceFileUnity<TSource>
        {
            TFile pngsUnity = ScriptableObject.CreateInstance<TFile>();
            pngsUnity.native = raw;
            int seqIndex = 0;
            pngsUnity.sequenceElements = new SequenceElement<TSource>[raw.Count];
            pngsUnity.loopCount = raw.Header.LoopCount;
            pngsUnity.preferredResolution = new Vector2Int((int)raw.Header.IHDR.Width, (int)raw.Header.IHDR.Height);
            outputSources = new TSource[raw.Count];
            List<UnityEngine.Object> potentialSubAssetsList = new List<UnityEngine.Object>();
            foreach (PngSequenceFile.SequenceElement rawSeq in raw)
            {
                SequenceElement<TSource> unitySeq = new SequenceElement<TSource>();
                TSource source = pngsUnity.CreateSourceFromNativeElement(rawSeq, out UnityEngine.Object potentialSubAsset);
                potentialSubAssetsList.Add(potentialSubAsset);
                unitySeq.source = source;
                unitySeq.length = rawSeq.Length;
                pngsUnity.sequenceElements[seqIndex] = unitySeq;
                outputSources[seqIndex] = source;
                seqIndex++;
            }
            potentialSubAssets = potentialSubAssetsList.ToArray();
            return pngsUnity;
        }
        
    }
}
