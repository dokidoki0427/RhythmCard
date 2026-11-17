using System.Collections.Generic;
using CardRhythm.Battle;
using CardRhythm.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 地图。五线谱与 8 个音符槽位在预制体里摆好，
    /// 脚本只负责按当前进度给槽位换状态（颜色、标签、可点击）。
    /// </summary>
    public sealed class MapForm : UIFormBase
    {
        private static Dictionary<string, EnemyDefinition> s_Enemies;
        private readonly RectTransform[] m_Slots = new RectTransform[RunState.MaxLayer];

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            for (int i = 0; i < m_Slots.Length; i++)
            {
                m_Slots[i] = transform.Find("Node" + (i + 1)) as RectTransform;
            }

            Refresh();
        }

        public void Refresh()
        {
            SetText("Subtitle", "第 " + RunState.Layer + " 层 / " + RunState.MaxLayer + "   ·   " + NodeName(RunState.CurrentNode));

            for (int i = 0; i < m_Slots.Length; i++)
            {
                BuildNode(i + 1, m_Slots[i]);
            }
        }

        private void BuildNode(int layer, RectTransform slot)
        {
            if (slot == null)
            {
                return;
            }

            for (int i = slot.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.GetChild(i).gameObject);
            }

            MapNodeType type = RunState.Nodes[layer - 1];
            bool current = layer == RunState.Layer;
            bool done = layer < RunState.Layer;

            Color color;
            float size;

            if (current)
            {
                color = SheetTheme.Gold;
                size = 34f;
                SheetTheme.Note(slot, Vector2.zero, 62f, new Color(0.78f, 0.62f, 0.24f, 0.18f));
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

            SheetTheme.Note(slot, Vector2.zero, size, color);

            var image = slot.GetComponent<Image>();
            if (image == null)
            {
                image = slot.gameObject.AddComponent<Image>();
            }

            image.raycastTarget = current;
            image.color = new Color(1f, 1f, 1f, 0f);

            var button = slot.GetComponent<Button>();
            if (button == null)
            {
                button = slot.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = image;
            button.onClick.RemoveAllListeners();
            button.interactable = current;
            if (current)
            {
                button.onClick.AddListener(Advance);
            }

            UIWidgets.Label(slot, NodeLabel(layer, type), 24, TextAnchor.MiddleCenter,
                current ? SheetTheme.Gold : new Color(0.95f, 0.90f, 0.80f, 0.6f),
                new Vector2(240f, 56f), new Vector2(0f, -66f));

            UIWidgets.Label(slot, layer.ToString(), 22, TextAnchor.MiddleCenter,
                current ? SheetTheme.Gold : new Color(0.95f, 0.90f, 0.80f, 0.35f),
                new Vector2(60f, 28f), new Vector2(0f, 56f));

            if (done)
            {
                UIWidgets.Label(slot, "✓", 30, TextAnchor.MiddleCenter, new Color(0.55f, 0.50f, 0.42f, 0.7f),
                    new Vector2(50f, 40f), new Vector2(0f, 46f));
            }
        }

        private static string NodeLabel(int layer, MapNodeType type)
        {
            string label = NodeName(type);
            string enemyId = RunState.EnemyIds[layer - 1];
            if (!string.IsNullOrEmpty(enemyId))
            {
                EnemyDefinition enemy = LoadEnemy(enemyId);
                if (enemy != null)
                {
                    label += "\n" + enemy.DisplayName;
                }
            }

            return label;
        }

        private static EnemyDefinition LoadEnemy(string id)
        {
            if (s_Enemies == null)
            {
                s_Enemies = new Dictionary<string, EnemyDefinition>();
            }

            EnemyDefinition cached;
            if (s_Enemies.TryGetValue(id, out cached))
            {
                return cached;
            }

            var enemy = Resources.Load<EnemyDefinition>("Enemies/" + id);
            s_Enemies[id] = enemy;
            return enemy;
        }

        private void SetText(string childName, string value)
        {
            Transform child = transform.Find(childName);
            if (child == null)
            {
                return;
            }

            var text = child.GetComponent<Text>();
            if (text != null)
            {
                text.text = value;
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
    }
}
