using UnityEngine;

namespace CardRhythm.Cards
{
    public enum EnemyBehavior
    {
        Attacker = 0,   // 每 N 次非完美判定攻击一次
        Disruptor = 1,  // 攻击更频繁，并且每 N 小节固定掉血
        Shielded = 2,   // 有护盾，只有强拍牌能破盾，破盾前本体免伤
    }

    /// <summary>
    /// 敌人模板。数值与行为都在这里，BattleState 只负责执行。
    /// </summary>
    [CreateAssetMenu(menuName = "CardRhythm/Enemy", fileName = "enemy_new")]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string m_Id = "enemy_new";
        [SerializeField] private string m_DisplayName = "敌人";
        [SerializeField] private int m_MaxHp = 300;
        [SerializeField] private int m_Attack = 10;
        [SerializeField] private int m_NonPerfectPerAttack = 2;
        [SerializeField] private EnemyBehavior m_Behavior = EnemyBehavior.Attacker;
        [SerializeField] private int m_ShieldHp = 0;
        [SerializeField] private int m_PeriodicBars = 8;
        [SerializeField] private int m_PeriodicDamage = 0;

        public string Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        public string DisplayName
        {
            get { return m_DisplayName; }
            set { m_DisplayName = value; }
        }

        public int MaxHp
        {
            get { return m_MaxHp; }
            set { m_MaxHp = Mathf.Max(1, value); }
        }

        public int Attack
        {
            get { return m_Attack; }
            set { m_Attack = Mathf.Max(0, value); }
        }

        public int NonPerfectPerAttack
        {
            get { return m_NonPerfectPerAttack; }
            set { m_NonPerfectPerAttack = Mathf.Max(1, value); }
        }

        public EnemyBehavior Behavior
        {
            get { return m_Behavior; }
            set { m_Behavior = value; }
        }

        public int ShieldHp
        {
            get { return m_ShieldHp; }
            set { m_ShieldHp = Mathf.Max(0, value); }
        }

        public int PeriodicBars
        {
            get { return m_PeriodicBars; }
            set { m_PeriodicBars = Mathf.Max(1, value); }
        }

        public int PeriodicDamage
        {
            get { return m_PeriodicDamage; }
            set { m_PeriodicDamage = Mathf.Max(0, value); }
        }
    }
}
