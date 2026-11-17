using CardRhythm.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 地图：一条横向五线谱，8 个音符代表 8 层。
    /// 已完成为暗色、当前为金色并带光环、未来为浅色；点击当前音符进入。
    /// </summary>
    public sealed class MapForm : UIFormBase
    {
        private RectTransform m_Root;
        private bool m_Built;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (!m_Built)
            {
                BuildUi();
                m_Built = true;
            }

            var background = GetComponent<Image>();
            if (background != null)
            {
                background.color = SheetTheme.Smoke;
            }

            HideTemplateChild("Title");
            HideTemplateChild("Hint");
            Refresh();
        }

        private void BuildUi()
        {
            m_Root = SheetTheme.CreateRect(transform, "MapRoot");
            m_Root.anchorMin = Vector2.zero;
            m_Root.anchorMax = Vector2.one;
            m_Root.sizeDelta = Vector2.zero;
            m_Root.anchoredPosition = Vector2.zero;
        }

        public void Refresh()
        {
            for (int i = m_Root.childCount - 1; i >= 0; i--)
            {
                Destroy(m_Root.GetChild(i).gameObject);
            }

            UIWidgets.Label(m_Root, "行 板", 54, TextAnchor.MiddleCenter, SheetTheme.Gold,
                new Vector2(600f, 70f), new Vector2(0f, 400f));
            UIWidgets.Label(m_Root, "第 " + RunState.Layer + " 层 / " + RunState.MaxLayer + "   ·   " + NodeName(RunState.CurrentNode),
                28, TextAnchor.MiddleCenter, new Color(0.95f, 0.90f, 0.80f, 0.75f),
                new Vector2(900f, 40f), new Vector2(0f, 345f));

            // 横向五线谱
            SheetTheme.Staff(m_Root, new Vector2(0f, 0f), 1560f, 30f, new Color(0.78f, 0.62f, 0.24f, 0.45f));

            // 连接线（谱线中央的一条细线）
            SheetTheme.Fill(m_Root, "Path", new Vector2(1500f, 1f), new Vector2(0f, 0f), new Color(0.78f, 0.62f, 0.24f, 0.25f));

            for (int layer = 1; layer <= RunState.MaxLayer; layer++)
            {
                BuildNode(layer);
            }

            UIWidgets.Label(m_Root, "Space / 点击金色音符进入   ·   B 牌组   ·   O 设置",
                22, TextAnchor.MiddleCenter, new Color(0.95f, 0.90f, 0.80f, 0.55f),
                new Vector2(1200f, 40f), new Vector2(0f, -330f));
        }

        private void BuildNode(int layer)
        {
            MapNodeType type = RunState.Nodes[layer - 1];
            bool current = layer == RunState.Layer;
            bool done = layer < RunState.Layer;

            float x = -700f + (layer - 1) * 200f;
            float pitch = (layer % 5 - 2) * 30f;
            var position = new Vector2(x, pitch);

            Color color;
            float size;

            if (current)
            {
                color = SheetTheme.Gold;
                size = 34f;
                // 当前音符光环
                SheetTheme.Note(m_Root, position, 60f, new Color(0.78f, 0.62f, 0.24f, 0.18f));
            }
            else if (done)
            {
                color = new Color(0.55f, 0.50f, 0.42f, 0.55f);
                size = 20f;
            }
            else
            {
                color = new Color(0.95f, 0.90f, 0.80f, 0.75f);
                size = 24f;
            }

            SheetTheme.Note(m_Root, position, size, color);

            // 点击区
            var hit = SheetTheme.CreateRect(m_Root, "Hit" + layer);
            hit.anchorMin = new Vector2(0.5f, 0.5f);
            hit.anchorMax = new Vector2(0.5f, 0.5f);
            hit.sizeDelta = new Vector2(84f, 84f);
            hit.anchoredPosition = position + new Vector2(0f, 18f);
            var hitImage = hit.gameObject.AddComponent<Image>();
            hitImage.color = new Color(1f, 1f, 1f, 0f);
            var button = hit.gameObject.AddComponent<Button>();
            button.targetGraphic = hitImage;
            if (current)
            {
                button.onClick.AddListener(Advance);
            }
            else
            {
                button.interactable = false;
            }

            // 层号与类型
            UIWidgets.Label(m_Root, NodeName(type), 28, TextAnchor.MiddleCenter,
                current ? SheetTheme.Gold : new Color(0.95f, 0.90f, 0.80f, 0.6f),
                new Vector2(200f, 40f), new Vector2(x, pitch - 85f));

            Color indexColor = current ? SheetTheme.Gold : new Color(0.95f, 0.90f, 0.80f, 0.35f);
            UIWidgets.Label(m_Root, layer.ToString(), 22, TextAnchor.MiddleCenter, indexColor,
                new Vector2(50f, 30f), new Vector2(x, 95f));

            if (done)
            {
                UIWidgets.Label(m_Root, "✓", 30, TextAnchor.MiddleCenter, new Color(0.55f, 0.50f, 0.42f, 0.7f),
                    new Vector2(50f, 40f), position + new Vector2(0f, 45f));
            }
        }

        private static void Advance()
        {
            var procedure = CardRhythm.Procedure.GameProcedureBase.Current;
            if (procedure != null)
            {
                procedure.RequestAdvance();
            }
        }

        private static string NodeName(MapNodeType type)
        {
            switch (type)
            {
                case MapNodeType.Battle: return "战斗";
                case MapNodeType.Event: return "事件";
                case MapNodeType.Elite: return "精英";
                default: return "首领";
            }
        }

        private void HideTemplateChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
