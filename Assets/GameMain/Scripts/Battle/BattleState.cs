using CardRhythm.Cards;
using CardRhythm.Rhythm;
using UnityEngine;

namespace CardRhythm.Battle
{
    /// <summary>
    /// 一场战斗的数值状态（纯逻辑，可单测）。
    /// 规则：
    ///  - 玩家 100 血；敌人血量/攻击/间隔/护盾来自 EnemyDefinition；
    ///  - 每次判定按卡牌效果结算伤害，Miss 不结算（吞效果）；
    ///  - 强拍指定的卡伤害 ×1.5；
    ///  - 护盾型：护盾未破时只有强拍伤害生效，破盾溢出伤害打本体；
    ///  - 干扰型：每 N 小节固定掉血。
    /// </summary>
    public sealed class BattleState
    {
        public const float DefaultPlayerHp = 100f;

        public float PlayerHp = DefaultPlayerHp;
        public float EnemyHp;
        public float ShieldHp;
        public readonly float MaxEnemyHp;
        public readonly float MaxShieldHp;
        public readonly float EnemyAttack;
        public readonly int NonPerfectPerAttack;
        public readonly EnemyBehavior Behavior;
        public readonly int PeriodicBars;
        public readonly int PeriodicDamage;

        public int Score;
        public int Combo;
        public int MaxCombo;
        public int Perfect;
        public int Good;
        public int Bad;
        public int Miss;

        private int m_NonPerfect;
        private int m_LastPeriodicBar = -1;

        public BattleState()
            : this(300f, 10f, 2, EnemyBehavior.Attacker, 0f, 8, 0)
        {
        }

        public BattleState(EnemyDefinition enemy)
            : this(enemy != null ? enemy.MaxHp : 300,
                   enemy != null ? enemy.Attack : 10f,
                   enemy != null ? enemy.NonPerfectPerAttack : 2,
                   enemy != null ? enemy.Behavior : EnemyBehavior.Attacker,
                   enemy != null ? enemy.ShieldHp : 0f,
                   enemy != null ? enemy.PeriodicBars : 8,
                   enemy != null ? enemy.PeriodicDamage : 0)
        {
        }

        public BattleState(float enemyHp, float enemyAttack, int nonPerfectPerAttack, EnemyBehavior behavior,
            float shieldHp, int periodicBars, int periodicDamage)
        {
            EnemyHp = enemyHp;
            MaxEnemyHp = enemyHp;
            EnemyAttack = enemyAttack;
            NonPerfectPerAttack = Mathf.Max(1, nonPerfectPerAttack);
            Behavior = behavior;
            ShieldHp = Mathf.Max(0f, shieldHp);
            MaxShieldHp = ShieldHp;
            PeriodicBars = Mathf.Max(1, periodicBars);
            PeriodicDamage = Mathf.Max(0, periodicDamage);
        }

        public bool ShieldActive
        {
            get { return Behavior == EnemyBehavior.Shielded && ShieldHp > 0f; }
        }

        public bool IsVictory
        {
            get { return EnemyHp <= 0f; }
        }

        public bool IsDefeat
        {
            get { return PlayerHp <= 0f; }
        }

        public int JudgedCount
        {
            get { return Perfect + Good + Bad + Miss; }
        }

        public float Accuracy
        {
            get { return JudgedCount == 0 ? 0f : Score / (100f * JudgedCount); }
        }

        public void ApplyJudgement(Judgement judgement, float baseDamage, bool accent)
        {
            switch (judgement)
            {
                case Judgement.Perfect:
                    Perfect++;
                    Score += 100;
                    Combo++;
                    break;
                case Judgement.Good:
                    Good++;
                    Score += 70;
                    Combo++;
                    break;
                case Judgement.Bad:
                    Bad++;
                    Score += 40;
                    Combo++;
                    break;
                default:
                    Miss++;
                    Combo = 0;
                    break;
            }

            if (Combo > MaxCombo)
            {
                MaxCombo = Combo;
            }

            if (judgement != Judgement.Miss)
            {
                float damage = baseDamage * (accent ? 1.5f : 1f);
                if (ShieldActive)
                {
                    // 护盾未破：普通命中无效，强拍命中才削盾，溢出打本体。
                    if (accent)
                    {
                        ShieldHp -= damage;
                        if (ShieldHp < 0f)
                        {
                            EnemyHp += ShieldHp;
                            ShieldHp = 0f;
                        }
                    }
                }
                else
                {
                    EnemyHp -= damage;
                }
            }

            if (judgement != Judgement.Perfect)
            {
                m_NonPerfect++;
                if (m_NonPerfect >= NonPerfectPerAttack)
                {
                    m_NonPerfect = 0;
                    PlayerHp -= EnemyAttack;
                }
            }
        }

        /// <summary>每帧传入当前小节；干扰型敌人按周期扣血（同小节只触发一次）。</summary>
        public void TickBar(int bar)
        {
            if (Behavior != EnemyBehavior.Disruptor || PeriodicDamage <= 0)
            {
                return;
            }

            if (bar <= 0 || bar % PeriodicBars != 0 || bar == m_LastPeriodicBar)
            {
                return;
            }

            m_LastPeriodicBar = bar;
            PlayerHp -= PeriodicDamage;
        }

        public int DisplayPlayerHp
        {
            get { return Mathf.CeilToInt(Mathf.Max(0f, PlayerHp)); }
        }

        public int DisplayEnemyHp
        {
            get { return Mathf.CeilToInt(Mathf.Max(0f, EnemyHp)); }
        }

        public int DisplayShieldHp
        {
            get { return Mathf.CeilToInt(Mathf.Max(0f, ShieldHp)); }
        }
    }
}
