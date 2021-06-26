using UnityEditor;
using UnityEngine;

namespace NK.MyEditor
{
	[System.Serializable]
	public class SpriteAnimationReferences
	{
        public int animWidth = 160;
        public int animHeight = 160;
        public int numOfanimations = 8;
        public int numOfFrames = 3;
        public int samplesFrameRate = 5;
        public bool loop = true;

        public int pixelPerUnit = 16;
        public bool alphaIsTransparency = true;
        public TextureImporterCompression compression;
        public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
        public FilterMode filterMode;
    }
}