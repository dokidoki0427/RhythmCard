using CardRhythm.Rhythm;
using UnityEngine;

namespace CardRhythm.Battle
{
    /// <summary>
    /// 一场战斗的数值状态（纯逻辑，可单测）。
    /// 规则：
    ///  - 玩家 100 血、敌人 300 血、敌人每 2 次非完美判定攻击 10 点；
    ///  - 每次判定按卡牌效果结算伤害，Miss 不结算（吞效果）；
    ///  - 强拍指定的卡伤害 ×1.5。
    /// </summary>
    public sealed class BattleState
    {
        public float PlayerHp = 100f;
        public float EnemyHp = 300f;
        public float EnemyAttack = 10f;
        public int NonPerfectPerAttack = 2;

        public int Score;
        public int Combo;
        public int MaxCombo;
        public int Perfect;
        public int Good;
        public int Bad;
        public int Miss;

        private int m_NonPerfect;

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
                EnemyHp -= baseDamage * (accent ? 1.5f : 1f);
            }

            if (judgement != Judgement.Perfect)
            {
                m_NonPerfect++;
                if (m_NonPerfect >= Mathf.Max(1, NonPerfectPerAttack))
                {
                    m_NonPerfect = 0;
                    PlayerHp -= EnemyAttack;
                }
            }
        }

        public int DisplayPlayerHp
        {
            get { return Mathf.CeilToInt(Mathf.Max(0f, PlayerHp)); }
        }

        public int DisplayEnemyHp
        {
            get { return Mathf.CeilToInt(Mathf.Max(0f, EnemyHp)); }
        }
    }
}
