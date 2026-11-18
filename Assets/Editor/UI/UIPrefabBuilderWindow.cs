using System;
using System.IO;
using CardRhythm.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.EditorUI
{
    /// <summary>
    /// 界面预制体生成器：把每个界面生成成“可在编辑器里直接摆”的预制体。
    /// 静态装饰（标题、五线谱、按钮、遮罩、面板）都在预制体里；
    /// 只有列表类内容（牌组卡片、奖励三选一）仍由脚本运行时填充。
    /// </summary>
    public class UIPrefabBuilderWindow : EditorWindow
    {
        private Vector2 m_Scroll;

        [MenuItem("Tools/卡牌音游/UI/预制体生成器", false, 90)]
        public static void Open()
        {
            var window = GetWindow<UIPrefabBuilderWindow>("UI 预制体生成器");
            window.minSize = new Vector2(620f, 460f);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("每个界面生成标准预制体（根节点全屏 + 五线谱主题）", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            if (GUILayout.Button("重建全部界面预制体", GUILayout.Height(32f)))
            {
                BuildAll();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("UI 预制体", "已重建全部界面预制体。", "好");
            }

            EditorGUILayout.Space(8f);
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            foreach (UIFormDef def in UIFormCatalog.All)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(def.Id.ToString(), GUILayout.Width(110f));
                    EditorGUILayout.LabelField(def.AssetPath.Replace("Assets/GameMain/UIForms/", ""), GUILayout.Width(160f));
                    if (GUILayout.Button("重建", GUILayout.Width(60f)))
                    {
                        Build(def);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6f);
            EditorGUILayout.HelpBox(
                "规范：根节点必须挂 UIFormBase 派生脚本 + RectTransform 全屏；子节点命名固定（Title/Content/Panel/CloseButton）。" +
                "运行时由 UIFormBase 自动套用中文字体与全屏拉伸。",
                MessageType.Info);
        }

        [MenuItem("Tools/卡牌音游/UI/重建全部预制体", false, 91)]
        public static void BuildAllMenu()
        {
            BuildAll();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>批处理入口。</summary>
        public static void BuildAllBatch()
        {
            try
            {
                BuildAll();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[UI-BUILD] PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("[UI-BUILD] FAIL " + exception);
                EditorApplication.Exit(1);
            }
        }

        public static void BuildAll()
        {
            foreach (UIFormDef def in UIFormCatalog.All)
            {
                Build(def);
            }
        }

        public static void Build(UIFormDef def)
        {
            GameObject root = def.Id == UIFormId.Menu ? BuildMenu(def)
                : def.Id == UIFormId.Map ? BuildMap(def)
                : def.Id == UIFormId.BattleHud ? BuildHud(def)
                : def.Id == UIFormId.Result ? BuildResult(def)
                : BuildPopup(def);

            PrefabUtility.SaveAsPrefabAsset(root, def.AssetPath);
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log("[UI-BUILD] " + def.AssetPath);
        }

        // ── 各界面 ──────────────────────────────────────────────

        private static GameObject BuildMenu(UIFormDef def)
        {
            var root = CreateRoot(def, "MenuForm", new Color(0.95f, 0.90f, 0.80f, 1f));

            Text(root.transform, "Title", "卡牌音游", 110, new Vector2(0f, 430f), new Vector2(1200f, 180f), SheetTheme.Ink);
            Text(root.transform, "Subtitle", "DECK  IS  THE  SCORE", 30, new Vector2(0f, 330f), new Vector2(900f, 50f), new Color(0.15f, 0.13f, 0.11f, 0.55f));

            Staff(root.transform, new Vector2(0f, 235f), 1180f, 34f, new Color(0.15f, 0.13f, 0.11f, 0.35f));

            float[] noteX = { -420f, -210f, -40f, 160f, 360f };
            float[] noteY = { 201f, 235f, 269f, 167f, 303f };
            for (int i = 0; i < noteX.Length; i++)
            {
                Glyph(root.transform, new Vector2(noteX[i], noteY[i]), 78f, SheetTheme.Ink);
            }

            Rect(root.transform, "Rule", new Vector2(760f, 2f), new Vector2(0f, 130f), new Color(0.15f, 0.13f, 0.11f, 0.25f));

            Button(root.transform, "StartButton", "开 始 游 戏", new Vector2(460f, 92f), new Vector2(0f, 20f), SheetTheme.Ink, SheetTheme.Parchment, 32);
            Button(root.transform, "DeckButton", "B U F F", new Vector2(300f, 68f), new Vector2(0f, -90f), SheetTheme.ParchmentDark, SheetTheme.Ink, 26);
            Button(root.transform, "SettingsButton", "设 置", new Vector2(300f, 68f), new Vector2(0f, -178f), SheetTheme.ParchmentDark, SheetTheme.Ink, 26);
            Button(root.transform, "QuitButton", "退 出", new Vector2(300f, 68f), new Vector2(0f, -266f), new Color(0.86f, 0.80f, 0.67f, 0.6f), SheetTheme.Ink, 26);

            Text(root.transform, "Hint", "Space 开始 · B Buff · O 设置", 22, new Vector2(0f, -380f), new Vector2(900f, 40f), new Color(0.15f, 0.13f, 0.11f, 0.55f));
            return root;
        }

        private static GameObject BuildMap(UIFormDef def)
        {
            var root = CreateRoot(def, "MapForm", new Color(0.09f, 0.08f, 0.07f, 1f));

            Text(root.transform, "Title", "行 板", 54, new Vector2(0f, 400f), new Vector2(600f, 70f), SheetTheme.Gold);
            Text(root.transform, "Subtitle", "第 1 层 / 8", 28, new Vector2(0f, 345f), new Vector2(900f, 40f), new Color(0.95f, 0.90f, 0.80f, 0.75f));

            Staff(root.transform, new Vector2(0f, 0f), 1560f, 30f, new Color(0.78f, 0.62f, 0.24f, 0.45f));
            Rect(root.transform, "Path", new Vector2(1500f, 1f), new Vector2(0f, 0f), new Color(0.78f, 0.62f, 0.24f, 0.25f));

            for (int layer = 1; layer <= 8; layer++)
            {
                float x = -700f + (layer - 1) * 200f;
                float y = (layer % 5 - 2) * 30f;
                Rect(root.transform, "Node" + layer, new Vector2(90f, 90f), new Vector2(x, y), new Color(0f, 0f, 0f, 0f));
            }

            Text(root.transform, "Hint", "Space / 点击金色音符进入   ·   B 牌组   ·   O 设置", 22, new Vector2(0f, -330f), new Vector2(1200f, 40f), new Color(0.95f, 0.90f, 0.80f, 0.55f));
            return root;
        }

        private static GameObject BuildHud(UIFormDef def)
        {
            return CreateRoot(def, "BattleHudForm", new Color(0f, 0f, 0f, 0.25f));
        }

        private static GameObject BuildResult(UIFormDef def)
        {
            var root = CreateRoot(def, "ResultForm", new Color(0.06f, 0.09f, 0.13f, 0.92f));

            Text(root.transform, "Title", "RESULT", 72, new Vector2(0f, 430f), new Vector2(900f, 120f), SheetTheme.Gold);
            Text(root.transform, "Score", "SCORE 0", 36, new Vector2(0f, 318f), new Vector2(800f, 50f), Color.white);
            Text(root.transform, "Combo", "MAX COMBO 0", 30, new Vector2(0f, 252f), new Vector2(800f, 44f), new Color(0.85f, 0.85f, 0.85f));
            Text(root.transform, "Judgement", "PERFECT 0    GOOD 0    BAD 0    MISS 0", 28,
                new Vector2(0f, 190f), new Vector2(1200f, 44f), new Color(0.85f, 0.85f, 0.85f));
            Text(root.transform, "Accuracy", "ACCURACY 0.0%", 32, new Vector2(0f, 122f), new Vector2(800f, 46f), SheetTheme.Gold);

            Button(root.transform, "ContinueButton", "返回地图", new Vector2(320f, 76f), new Vector2(0f, -40f),
                SheetTheme.Gold, SheetTheme.Ink, 28);

            Rect(root.transform, "Content", new Vector2(1200f, 260f), new Vector2(0f, -260f), new Color(0f, 0f, 0f, 0f));
            return root;
        }

        private static GameObject BuildPopup(UIFormDef def)
        {
            Color panelColor = def.Id == UIFormId.Reward ? new Color(0.08f, 0.09f, 0.12f, 0.98f)
                : def.Id == UIFormId.Deck ? new Color(0.09f, 0.10f, 0.13f, 0.98f)
                : def.Id == UIFormId.Pause ? new Color(0.08f, 0.09f, 0.12f, 0.98f)
                : new Color(0.10f, 0.11f, 0.14f, 0.98f);

            Vector2 panelSize = def.Id == UIFormId.Deck || def.Id == UIFormId.Reward
                ? new Vector2(1240f, 820f)
                : def.Id == UIFormId.Pause ? new Vector2(720f, 560f) : new Vector2(920f, 620f);

            var root = CreateRoot(def, def.Id + "Form", new Color(0f, 0f, 0f, 0f));

            var backdrop = Rect(root.transform, "Backdrop", new Vector2(1920f, 1080f), Vector2.zero, new Color(0f, 0f, 0f, 0.55f));
            backdrop.anchorMin = Vector2.zero;
            backdrop.anchorMax = Vector2.one;
            backdrop.sizeDelta = Vector2.zero;

            var panel = Rect(root.transform, "Panel", panelSize, Vector2.zero, panelColor);
            var panelImage = panel.GetComponent<Image>();
            panelImage.sprite = Resources.Load<Sprite>("Pic/PanelFrame");
            if (panelImage.sprite != null)
            {
                panelImage.type = Image.Type.Sliced;
            }

            var content = Rect(panel, "Content", new Vector2(panelSize.x - 80f, panelSize.y - 120f), new Vector2(0f, -10f), new Color(0f, 0f, 0f, 0f));
            content.name = "Content";
            return root;
        }

        // ── 基础构件 ────────────────────────────────────────────

        private static GameObject CreateRoot(UIFormDef def, string name, Color background)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)root.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = root.GetComponent<Image>();
            image.color = background;
            image.raycastTarget = false;

            Type formType = UISetup.FormTypeOf(def.Id);
            if (formType == null)
            {
                throw new InvalidOperationException("没有界面脚本：" + def.Id);
            }

            root.AddComponent(formType);
            return root;
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 position, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static Text Text(Transform parent, string name, string content, int size, Vector2 position, Vector2 boxSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = boxSize;
            rect.anchoredPosition = position;

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            return text;
        }

        private static Button Button(Transform parent, string name, string label, Vector2 size, Vector2 position,
            Color background, Color textColor, int fontSize)
        {
            var rect = Rect(parent, name, size, position, background);
            var image = rect.GetComponent<Image>();
            image.raycastTarget = true; // 按钮必须可接收射线，否则点不动
            image.sprite = Resources.Load<Sprite>("Pic/ButtonFrame");
            if (image.sprite != null)
            {
                image.type = Image.Type.Sliced;
            }

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var text = Text(rect, "Label", label, fontSize, Vector2.zero, size, textColor);
            text.raycastTarget = false;
            return button;
        }

        private static void Staff(Transform parent, Vector2 center, float width, float gap, Color color)
        {
            for (int line = 0; line < 5; line++)
            {
                float y = center.y + (2 - line) * gap;
                Rect(parent, "Staff" + line, new Vector2(width, 2f), new Vector2(center.x, y), color);
            }
        }

        private static void Glyph(Transform parent, Vector2 position, float height, Color color)
        {
            var sprite = Resources.Load<Sprite>("Pic/NoteGlyph");
            if (sprite == null)
            {
                Note(parent, position, height * 0.5f, color);
                return;
            }

            float width = height * sprite.rect.width / sprite.rect.height;
            var rect = Rect(parent, "NoteGlyph", new Vector2(width, height), position, color);
            var image = rect.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
        }

        private static void Note(Transform parent, Vector2 position, float size, Color color)
        {
            var head = Rect(parent, "NoteHead", new Vector2(size, size * 0.78f), position, color);
            var headImage = head.GetComponent<Image>();
            headImage.sprite = Resources.Load<Sprite>("Pic/NoteHead") ?? UISpriteSetup.EnsureCircle();
            headImage.type = Image.Type.Simple;
            headImage.preserveAspect = true;

            var stem = Rect(parent, "Stem", new Vector2(4f, size * 2.2f), position + new Vector2(size * 0.45f, size * 1.1f), color);
            var stemImage = stem.GetComponent<Image>();
            stemImage.sprite = Resources.Load<Sprite>("Pic/Stem");
            if (stemImage.sprite != null)
            {
                stemImage.type = Image.Type.Sliced;
            }

            stem.pivot = new Vector2(0f, 1f);
        }
    }
}
