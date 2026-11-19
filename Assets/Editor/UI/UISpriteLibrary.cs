using System.IO;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorUI
{
    /// <summary>
    /// 生成界面需要的全部图片资源（白图 + 透明通道，靠 Image.color 染色）。
    /// 输出到 Assets/GameMain/Resources/UI，运行时可用 Resources.Load 取。
    /// 菜单：Tools → 卡牌音游 → UI → 生成图片资源
    /// </summary>
    public static class UISpriteLibrary
    {
        private const string Folder = "Assets/GameMain/Resources/Pic";

        [MenuItem("Tools/卡牌音游/UI/生成图片资源", false, 92)]
        public static void GenerateMenu()
        {
            int created = EnsureAll();
            EditorUtility.DisplayDialog("UI 图片", "新增 " + created + " 张图片资源。", "好");
        }

        public static void GenerateBatch()
        {
            int created = EnsureAll();
            Debug.Log("[UI-SPRITES] created=" + created);
            EditorApplication.Exit(0);
        }

        public static int EnsureAll()
        {
            EnsureFolder();
            int before = Count();

            CreateEllipse("NoteHead", 128, 96, false);
            CreateEllipse("NoteHeadOutline", 128, 96, true);
            CreateEllipse("Circle", 128, 128, false);
            CreateRounded("Stem", 12, 160, 6, Vector4.zero);
            CreateRounded("ButtonFrame", 192, 96, 26, new Vector4(26, 26, 26, 26));
            CreateRounded("CardFrame", 220, 300, 22, new Vector4(22, 22, 22, 22));
            CreateRounded("PanelFrame", 256, 256, 28, new Vector4(28, 28, 28, 28));
            CreateRounded("BarFrame", 256, 32, 14, new Vector4(14, 14, 14, 14));

            // 外部放进来的图（例如美术下载的音符）也要保证是 Sprite 导入。
            EnsureSprite(Folder + "/NoteGlyph.png");
            EnsureSprite(Folder + "/音符.png");
            EnsureSprite(Folder + "/BuffAttack.png");
            EnsureSprite(Folder + "/BuffDefense.png");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return Count() - before;
        }

        /// <summary>把已存在的图片强制设为 Sprite 导入（保留 GUID）。</summary>
        public static void EnsureSprite(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(path) == null)
            {
                return;
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            if (importer.textureType == TextureImporterType.Sprite
                && importer.spriteImportMode == SpriteImportMode.Single
                && importer.alphaIsTransparency)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            importer.SaveAndReimport();
        }

        // ── 形状 ────────────────────────────────────────────────

        private static void CreateEllipse(string name, int width, int height, bool outline)
        {
            var texture = NewTexture(width, height);
            float rx = width * 0.5f;
            float ry = height * 0.5f;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x + 0.5f - rx) / rx;
                    float ny = (y + 0.5f - ry) / ry;
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    float alpha = Mathf.Clamp01((1f - d) * Mathf.Min(rx, ry) * 0.5f);
                    if (outline)
                    {
                        alpha *= 1f - Mathf.Clamp01((0.72f - d) * Mathf.Min(rx, ry) * 0.5f);
                    }

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            Save(texture, name, Vector4.zero);
        }

        private static void CreateRounded(string name, int width, int height, int radius, Vector4 border)
        {
            var texture = NewTexture(width, height);
            float hw = width * 0.5f;
            float hh = height * 0.5f;
            float r = radius;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float px = x + 0.5f - hw;
                    float py = y + 0.5f - hh;
                    float dx = Mathf.Max(Mathf.Abs(px) - (hw - r), 0f);
                    float dy = Mathf.Max(Mathf.Abs(py) - (hh - r), 0f);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) - r;
                    float alpha = Mathf.Clamp01(0.5f - dist);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            Save(texture, name, border);
        }

        // ── 保存与导入 ──────────────────────────────────────────

        private static Texture2D NewTexture(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        private static void Save(Texture2D texture, string name, Vector4 border)
        {
            string path = Folder + "/" + name + ".png";
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) != null)
            {
                Object.DestroyImmediate(texture);
                return;
            }

            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.spritePixelsPerUnit = 100f;
                importer.spriteBorder = border;
                importer.maxTextureSize = 512;
                importer.SaveAndReimport();
            }
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameMain/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/GameMain", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets/GameMain/Resources", "UI");
            }
        }

        private static int Count()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                return 0;
            }

            return AssetDatabase.FindAssets("t:Sprite", new[] { Folder }).Length;
        }
    }
}
