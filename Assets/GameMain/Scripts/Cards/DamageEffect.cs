using UnityEngine;

namespace CardRhythm.Cards
{
    /// <summary>伤害效果：卡牌的功能数值放在这里，而不是卡牌资产上。</summary>
    [CreateAssetMenu(menuName = "CardRhythm/Effects/Damage", fileName = "damage_effect")]
    public class DamageEffect : CardEffect
    {
        [SerializeField, Min(0)]
        private int m_Damage = 5;

        public int Damage
        {
            get { return m_Damage; }
            set { m_Damage = Mathf.Max(0, value); }
        }

        public override string Summary
        {
            get { return "造成 " + m_Damage + " 点伤害"; }
        }
    }
}
