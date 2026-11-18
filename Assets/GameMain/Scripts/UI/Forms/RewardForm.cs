using System.Linq;
using CardRhythm.Cards;
using CardRhythm.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 战斗胜利后的三选一奖励：随机 3 张不同卡牌，点击加入牌组。
    /// </summary>
    public sealed class RewardForm : UIFormBase
    {
        private RectTransform m_Panel;
        private RectTransform m_Slots;
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
            m_Panel = transform.Find("Panel") as RectTransform;
            if (m_Panel == null)
            {
                m_Panel = UIWidgets.Panel(transform, "RewardPanel", new Vector2(1240f, 820f), Vector2.zero, SheetTheme.Parchment);
            }

            var panelImage = m_Panel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = SheetTheme.Parchment;
            }

            UIWidgets.Label(m_Panel, "选 择 一 个 B U F F", 44, TextAnchor.MiddleCenter, SheetTheme.Ink,
                new Vector2(900f, 60f), new Vector2(0f, 340f));
            UIWidgets.Label(m_Panel, "自动填进谱面槽位 · 满 3 个会替换最早的一个", 24, TextAnchor.MiddleCenter,
                SheetTheme.InkSoft, new Vector2(900f, 40f), new Vector2(0f, 290f));

            m_Slots = UIWidgets.Panel(m_Panel, "Slots", new Vector2(1180f, 440f), new Vector2(0f, 20f),
                new Color(0f, 0f, 0f, 0f));

            UIWidgets.Button(m_Panel, "跳过", new Vector2(200f, 60f), new Vector2(0f, -330f), Close,
                SheetTheme.ParchmentDark, SheetTheme.Ink, 24);
        }

        private void Refresh()
        {
            for (int i = m_Slots.childCount - 1; i >= 0; i--)
            {
                Destroy(m_Slots.GetChild(i).gameObject);
            }

            var cards = Resources.LoadAll<CardDefinition>("Buffs")
                .Where(c => c != null)
                .OrderBy(c => c.Id)
                .ToList();

            if (cards.Count == 0)
            {
                return;
            }

            var picks = new System.Collections.Generic.List<CardDefinition>(3);
            var pool = new System.Collections.Generic.List<CardDefinition>(cards);
            while (picks.Count < 3 && pool.Count > 0)
            {
                int index = Random.Range(0, pool.Count);
                picks.Add(pool[index]);
                pool.RemoveAt(index);
            }

            for (int i = 0; i < picks.Count; i++)
            {
                CardDefinition card = picks[i];
                float x = (i - (picks.Count - 1) * 0.5f) * 380f;

                var damageEffect = card.Effect as DamageEffect;
                int damage = damageEffect != null ? damageEffect.Damage : 0;
                string label = card.DisplayName + "\n\n" + card.NoteCount + " 音 · " + damage + " 伤"
                    + "\n\n" + card.Pattern;

                string id = card.Id;
                Color background = card.Accent;
                UIWidgets.Button(m_Slots, label, new Vector2(340f, 400f), new Vector2(x, 0f), () => Pick(id),
                    background, SheetTheme.Ink, 28);
            }
        }

        private void Pick(string cardId)
        {
            GameSettings.AppendToDeck(cardId);
            Close();
        }
    }
}
