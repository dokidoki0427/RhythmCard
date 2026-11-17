using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 五线谱视觉主题：羊皮纸、墨色、金线。所有界面共用这套颜色与字体。
    /// 没有美术资源，音符/线/圆点都用程序生成的基础图形拼。
    /// </summary>
    public static class SheetTheme
    {
        public static readonly Color Parchment = new Color(0.95f, 0.90f, 0.80f, 1f);
        public static readonly Color ParchmentDark = new Color(0.86f, 0.80f, 0.67f, 1f);
        public static readonly Color Ink = new Color(0.15f, 0.13f, 0.11f, 1f);
        public static readonly Color InkSoft = new Color(0.15f, 0.13f, 0.11f, 0.55f);
        public static readonly Color Gold = new Color(0.78f, 0.62f, 0.24f, 1f);
        public static readonly Color Crimson = new Color(0.55f, 0.18f, 0.20f, 1f);
        public static readonly Color Smoke = new Color(0.09f, 0.08f, 0.07f, 0.97f);

        private static Font s_Font;
        private static Sprite s_Circle;

        /// <summary>优先用系统中文字体，避免内置字体缺字变方块。</summary>
        public static Font MainFont
        {
            get
            {
                if (s_Font == null)
                {
                    s_Font = UnityEngine.Font.CreateDynamicFontFromOSFont(
                        new[] { "Microsoft YaHei", "Microsoft YaHei UI", "SimHei", "SimSun", "Arial" }, 32);
                }

                return s_Font;
            }
        }

        public static Sprite Circle
        {
            get
            {
                Sprite asset = Load("UI/Circle");
                if (asset != null)
                {
                    return asset;
                }

                if (s_Circle == null)
                {
                    s_Circle = CreateCircleSprite(64);
                }

                return s_Circle;
            }
        }

        public static Sprite NoteHead
        {
            get { return Load("UI/NoteHead") ?? Circle; }
        }

        public static Sprite NoteHeadOutline
        {
            get { return Load("UI/NoteHeadOutline") ?? Circle; }
        }

        public static Sprite Stem
        {
            get { return Load("UI/Stem"); }
        }

        public static Sprite ButtonFrame
        {
            get { return Load("UI/ButtonFrame"); }
        }

        public static Sprite PanelFrame
        {
            get { return Load("UI/PanelFrame"); }
        }

        public static Sprite CardFrame
        {
            get { return Load("UI/CardFrame"); }
        }

        public static Sprite BarFrame
        {
            get { return Load("UI/BarFrame"); }
        }

        private static Sprite Load(string path)
        {
            return Resources.Load<Sprite>(path);
        }

        public static void Fill(Transform parent, string name, Vector2 size, Vector2 position, Color color)
        {
            var rect = CreateRect(parent, name);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        /// <summary>五线谱：5 条横线。</summary>
        public static void Staff(Transform parent, Vector2 center, float width, float gap, Color color)
        {
            for (int line = 0; line < 5; line++)
            {
                float y = center.y + (2 - line) * gap;
                Fill(parent, "Staff" + line, new Vector2(width, 2f), new Vector2(center.x, y), color);
            }
        }

        /// <summary>一个音符：符头（圆）+ 符干（竖线）。</summary>
        public static RectTransform Note(Transform parent, Vector2 position, float size, Color color)
        {
            var head = CreateRect(parent, "NoteHead");
            head.anchorMin = new Vector2(0.5f, 0.5f);
            head.anchorMax = new Vector2(0.5f, 0.5f);
            head.sizeDelta = new Vector2(size, size * 0.78f);
            head.anchoredPosition = position;

            var image = head.gameObject.AddComponent<Image>();
            image.sprite = NoteHead;
            image.preserveAspect = true;
            image.color = color;
            image.raycastTarget = false;

            var stem = CreateRect(parent, "Stem");
            stem.anchorMin = new Vector2(0.5f, 0.5f);
            stem.anchorMax = new Vector2(0.5f, 0.5f);
            stem.sizeDelta = new Vector2(4f, size * 2.2f);
            stem.anchoredPosition = position + new Vector2(size * 0.45f, size * 1.1f);
            var stemImage = stem.gameObject.AddComponent<Image>();
            if (Stem != null)
            {
                stemImage.sprite = Stem;
                stemImage.type = Image.Type.Sliced;
            }

            stemImage.color = color;
            stemImage.raycastTarget = false;

            return head;
        }

        public static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Sprite CreateCircleSprite(int size)
        {
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
                    float alpha = Mathf.Clamp01(radius - distance);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
