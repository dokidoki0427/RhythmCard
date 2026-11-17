using System.IO;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorUI
{
    /// <summary>生成界面用的基础 Sprite（圆形音符头），存成真实资源，预制体里能直接看到。</summary>
    public static class UISpriteSetup
    {
        private const string Folder = "Assets/GameMain/UI";
        private const string CirclePath = Folder + "/Circle.png";

        public static Sprite EnsureCircle()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
            if (existing != null)
            {
                return existing;
            }

            if (!AssetDatabase.IsValidFolder(Folder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/GameMain"))
                {
                    AssetDatabase.CreateFolder("Assets", "GameMain");
                }

                AssetDatabase.CreateFolder("Assets/GameMain", "UI");
            }

            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size * 0.5f;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - radius;
                    float dy = y + 0.5f - radius;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)(Mathf.Clamp01(radius - distance) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            File.WriteAllBytes(CirclePath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(CirclePath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(CirclePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
        }
    }
}
