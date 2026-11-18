using UnityEngine;

namespace CardRhythm.Cards
{
    /// <summary>治疗：按判定强度回复玩家生命。</summary>
    [CreateAssetMenu(menuName = "CardRhythm/Effects/Heal", fileName = "heal_effect")]
    public class HealEffect : CardEffect
    {
        [SerializeField, Min(0)]
        private int m_Heal = 8;

        public int Heal
        {
            get { return m_Heal; }
            set { m_Heal = Mathf.Max(0, value); }
        }

        public override string Summary
        {
            get { return "回复 " + m_Heal + " 点生命"; }
        }
    }
}
