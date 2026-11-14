using CardRhythm.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>地图：显示 8 层进度与当前节点，Space 进入下一层。</summary>
    public sealed class MapForm : UIFormBase
    {
        private RectTransform m_Panel;
        private RectTransform m_Rows;
        private bool m_Built;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (!m_Built)
            {
                BuildUi();
                m_Built = true;
            }

            Refresh();
        }

        private void BuildUi()
        {
            m_Panel = UIWidgets.Panel(transform, "MapPanel", new Vector2(1100f, 900f), Vector2.zero,
                new Color(0.07f, 0.08f, 0.11f, 0.97f));

            UIWidgets.Label(m_Panel, "MAP", 44, TextAnchor.MiddleCenter, Color.white,
                new Vector2(800f, 60f), new Vector2(0f, 390f));

            m_Rows = UIWidgets.Panel(m_Panel, "Rows", new Vector2(900f, 700f), new Vector2(0f, 0f),
                new Color(0f, 0f, 0f, 0f));

            UIWidgets.Label(m_Panel, "Space 进入下一层", 24, TextAnchor.MiddleCenter,
                new Color(0.8f, 0.8f, 0.8f), new Vector2(800f, 40f), new Vector2(0f, -400f));
        }

        public void Refresh()
        {
            for (int i = m_Rows.childCount - 1; i >= 0; i--)
            {
                Destroy(m_Rows.GetChild(i).gameObject);
            }

            for (int layer = 1; layer <= RunState.MaxLayer; layer++)
            {
                MapNodeType type = RunState.Nodes[layer - 1];
                bool current = layer == RunState.Layer;
                bool done = layer < RunState.Layer;

                Color color;
                string suffix;

                if (current)
                {
                    color = new Color(0.2f, 0.32f, 0.45f, 1f);
                    suffix = "  ← 当前";
                }
                else if (done)
                {
                    color = new Color(0.14f, 0.15f, 0.18f, 1f);
                    suffix = "  (已完成)";
                }
                else
                {
                    color = new Color(0.1f, 0.11f, 0.14f, 1f);
                    suffix = string.Empty;
                }

                var row = UIWidgets.Panel(m_Rows, "Layer" + layer, new Vector2(760f, 66f),
                    new Vector2(0f, 310f - (layer - 1) * 86f), color);

                UIWidgets.Label(row, (layer + ". " + type + suffix), 28, TextAnchor.MiddleLeft,
                    current ? Color.white : new Color(0.75f, 0.75f, 0.75f), new Vector2(700f, 50f), new Vector2(30f, 0f));
            }
        }
    }
}
