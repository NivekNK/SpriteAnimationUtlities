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
                SetTextureImporter(ref ti, pixelPerUnit, compression, alphaIsTransparency, wrap, filter);

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

        public static void SetSpriteProperties(Texture2D texture, int pixelPerUnit = 0,
            TextureImporterCompression compression = TextureImporterCompression.Uncompressed, bool alphaIsTransparency = true,
            TextureWrapMode wrap = TextureWrapMode.Clamp, FilterMode filter = FilterMode.Point)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            SetTextureImporter(ref ti, pixelPerUnit, compression, alphaIsTransparency, wrap, filter, SpriteImportMode.Single);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        public static void SetTextureImporter(ref TextureImporter ti, int pixelPerUnit = 0,
            TextureImporterCompression compression = TextureImporterCompression.Uncompressed, bool alphaIsTransparency = true,
            TextureWrapMode wrap = TextureWrapMode.Clamp, FilterMode filter = FilterMode.Point, SpriteImportMode importMode = SpriteImportMode.Multiple)
        {
            ti.isReadable = true;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = importMode;
            ti.alphaIsTransparency = alphaIsTransparency;
            if (pixelPerUnit != 0) ti.spritePixelsPerUnit = pixelPerUnit;
            ti.wrapMode = wrap;
            ti.filterMode = filter;
            ti.textureCompression = compression;
        }
    }
}