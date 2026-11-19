using UnityEngine;

namespace CardRhythm.Cards
{
    /// <summary>防御 Buff：获得护甲。护甲会先于生命被敌人攻击消耗。</summary>
    [CreateAssetMenu(menuName = "CardRhythm/Effects/Armor", fileName = "armor_effect")]
    public class ArmorEffect : CardEffect
    {
        [SerializeField, Min(0)]
        private int m_Armor = 10;

        public int Armor
        {
            get { return m_Armor; }
            set { m_Armor = Mathf.Max(0, value); }
        }

        public override string Summary
        {
            get { return "获得 " + m_Armor + " 点护甲"; }
        }
    }
}
