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
            m_Panel = UIWidgets.Panel(transform, "RewardPanel", new Vector2(1240f, 820f), Vector2.zero,
                new Color(0.08f, 0.09f, 0.12f, 0.98f));

            UIWidgets.Label(m_Panel, "CHOOSE A CARD", 44, TextAnchor.MiddleCenter, Color.white,
                new Vector2(900f, 60f), new Vector2(0f, 340f));
            UIWidgets.Label(m_Panel, "点击一张加入牌组（可跳过）", 24, TextAnchor.MiddleCenter,
                new Color(0.8f, 0.8f, 0.8f), new Vector2(900f, 40f), new Vector2(0f, 290f));

            m_Slots = UIWidgets.Panel(m_Panel, "Slots", new Vector2(1180f, 440f), new Vector2(0f, 20f),
                new Color(0f, 0f, 0f, 0f));

            UIWidgets.Button(m_Panel, "跳过", new Vector2(200f, 60f), new Vector2(0f, -330f), Close,
                new Color(0.3f, 0.3f, 0.34f));
        }

        private void Refresh()
        {
            for (int i = m_Slots.childCount - 1; i >= 0; i--)
            {
                Destroy(m_Slots.GetChild(i).gameObject);
            }

            var cards = Resources.LoadAll<CardDefinition>("Cards")
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
                Color background = new Color(card.Accent.r * 0.45f, card.Accent.g * 0.45f, card.Accent.b * 0.45f, 1f);
                UIWidgets.Button(m_Slots, label, new Vector2(340f, 400f), new Vector2(x, 0f), () => Pick(id), background);
            }
        }

        private void Pick(string cardId)
        {
            GameSettings.AppendToDeck(cardId);
            Close();
        }
    }
}
