using System.Collections.Generic;
using System.Linq;
using CardRhythm.Cards;
using CardRhythm.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 牌组界面：从全部卡牌里选牌（上限 20 张），存 PlayerPrefs。
    /// 运行时用代码搭 UI，不依赖预制体布局。
    /// </summary>
    public sealed class DeckForm : UIFormBase
    {
        private const int DeckSize = 20;
        private const int Columns = 3;

        private readonly HashSet<string> m_Selected = new HashSet<string>();

        private RectTransform m_Grid;
        private Text m_CountLabel;
        private Text m_StatusLabel;
        private bool m_Built;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (!m_Built)
            {
                BuildUi();
                m_Built = true;
            }

            LoadSelection();
            Refresh();
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void BuildUi()
        {
            var panel = UIWidgets.Panel(transform, "DeckPanel", new Vector2(1240f, 820f), Vector2.zero,
                new Color(0.09f, 0.1f, 0.13f, 0.98f));

            UIWidgets.Label(panel, "DECK", 44, TextAnchor.MiddleCenter, Color.white,
                new Vector2(800f, 60f), new Vector2(0f, 350f));

            m_CountLabel = UIWidgets.Label(panel, "0 / 8", 28, TextAnchor.MiddleCenter,
                new Color(0.9f, 0.85f, 0.6f), new Vector2(400f, 40f), new Vector2(0f, 300f));

            m_Grid = UIWidgets.Panel(panel, "Grid", new Vector2(1180f, 560f), new Vector2(0f, 20f),
                new Color(0f, 0f, 0f, 0f));

            m_StatusLabel = UIWidgets.Label(panel, string.Empty, 24, TextAnchor.MiddleCenter,
                new Color(0.8f, 0.8f, 0.8f), new Vector2(800f, 40f), new Vector2(0f, -300f));

            UIWidgets.Button(panel, "保存", new Vector2(200f, 60f), new Vector2(-120f, -360f), Save, new Color(0.24f, 0.34f, 0.2f));
            UIWidgets.Button(panel, "关闭", new Vector2(200f, 60f), new Vector2(120f, -360f), Close, new Color(0.3f, 0.3f, 0.34f));
        }

        private void LoadSelection()
        {
            m_Selected.Clear();
            List<string> saved = GameSettings.LoadDeck();
            List<CardDefinition> cards = LoadCards();

            if (saved.Count == 0)
            {
                foreach (CardDefinition card in cards)
                {
                    if (m_Selected.Count < DeckSize)
                    {
                        m_Selected.Add(card.Id);
                    }
                }

                return;
            }

            var available = new HashSet<string>(cards.Select(c => c.Id));
            foreach (string id in saved)
            {
                if (available.Contains(id) && m_Selected.Count < DeckSize)
                {
                    m_Selected.Add(id);
                }
            }
        }

        private void Refresh()
        {
            if (m_Grid == null)
            {
                return;
            }

            for (int i = m_Grid.childCount - 1; i >= 0; i--)
            {
                Destroy(m_Grid.GetChild(i).gameObject);
            }

            List<CardDefinition> cards = LoadCards();
            for (int i = 0; i < cards.Count; i++)
            {
                CardDefinition card = cards[i];
                int row = i / Columns;
                int column = i % Columns;
                float x = (column - (Columns - 1) * 0.5f) * 380f;
                float y = 220f - row * 130f;

                bool selected = m_Selected.Contains(card.Id);
                var damageEffect = card.Effect as DamageEffect;
                int damage = damageEffect != null ? damageEffect.Damage : 0;
                string label = card.DisplayName + "\n" + card.NoteCount + " 音 · " + damage + " 伤";

                Color color = selected ? card.Accent : new Color(0.2f, 0.21f, 0.25f, 1f);
                string id = card.Id;
                UIWidgets.Button(m_Grid, label, new Vector2(350f, 110f), new Vector2(x, y), () => Toggle(id), color);
            }

            if (m_CountLabel != null)
            {
                m_CountLabel.text = m_Selected.Count + " / " + DeckSize;
            }
        }

        private void Toggle(string id)
        {
            if (m_Selected.Contains(id))
            {
                m_Selected.Remove(id);
            }
            else if (m_Selected.Count >= DeckSize)
            {
                SetStatus("最多 " + DeckSize + " 张，先取消一张");
                return;
            }
            else
            {
                m_Selected.Add(id);
            }

            SetStatus(string.Empty);
            Refresh();
        }

        private void Save()
        {
            if (m_Selected.Count == 0)
            {
                SetStatus("至少要选 1 张");
                return;
            }

            GameSettings.SaveDeck(m_Selected);
            SetStatus("已保存：" + m_Selected.Count + " 张");
        }

        private static List<CardDefinition> LoadCards()
        {
            var cards = new List<CardDefinition>(Resources.LoadAll<CardDefinition>("Cards"));
            cards.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            return cards;
        }

        private void SetStatus(string text)
        {
            if (m_StatusLabel != null)
            {
                m_StatusLabel.text = text;
            }
        }
    }
}
