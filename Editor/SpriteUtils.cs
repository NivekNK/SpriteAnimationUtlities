using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NK.MyEditor
{
    public static class SpriteUtils
    {
        public static void SliceSprites(int sliceWidth, int sliceHeight, Texture2D[] spriteSheets, int pixelPerUnit = 0,
            TextureImporterCompression compression = TextureImporterCompression.Uncompressed, bool alphaIsTransparency = true,
            TextureWrapMode wrap = TextureWrapMode.Clamp, FilterMode filter = FilterMode.Point)
        {
            for (int z = 0; z < spriteSheets.Length; z++)
            {
                string path = AssetDatabase.GetAssetPath(spriteSheets[z]);
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                ti.isReadable = true;
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Multiple;
                ti.alphaIsTransparency = alphaIsTransparency;
                if (pixelPerUnit != 0) ti.spritePixelsPerUnit = pixelPerUnit;
                ti.wrapMode = wrap;
                ti.filterMode = filter;
                ti.textureCompression = compression;

                List<SpriteMetaData> newData = new List<SpriteMetaData>();

                Texture2D spriteSheet = spriteSheets[z];

                for (int i = 0; i < spriteSheet.width; i += sliceWidth)
                {
                    for (int j = spriteSheet.height; j > 0; j -= sliceHeight)
                    {
                        SpriteMetaData smd = new SpriteMetaData
                        {
                            pivot = new Vector2(0.5f, 0.5f),
                            alignment = 9,
                            name = spriteSheet.name + "_" + ((spriteSheet.height - j) / sliceHeight) + "_" + (i / sliceWidth),
                            rect = new Rect(i, j - sliceHeight, sliceWidth, sliceHeight)
                        };

                        newData.Add(smd);
                    }
                }

                ti.spritesheet = newData.ToArray();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            Debug.Log("Done Slicing!");
        }
    }
}