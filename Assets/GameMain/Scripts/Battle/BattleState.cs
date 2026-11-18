using System.Collections.Generic;
using CardRhythm.Cards;
using CardRhythm.Rhythm;
using UnityEngine;

namespace CardRhythm.Battle
{
    /// <summary>一个 Buff 在本场战斗里的进度：需要判定几个音符、已经判定几个、累计权重。</summary>
    public sealed class BuffProgress
    {
        public string Id;
        public string DisplayName;
        public Color Color;
        public int Required;
        public int Judged;
        public float Weight;
        public CardDefinition Definition;

        public bool Complete
        {
            get { return Judged >= Required; }
        }

        /// <summary>0..1，判定越准越高。</summary>
        public float Activation
        {
            get { return Required <= 0 ? 0f : Mathf.Clamp01(Weight / Required); }
        }
    }

    /// <summary>刚刚结算的 Buff，供 HUD 显示。</summary>
    public sealed class BuffActivation
    {
        public string DisplayName;
        public Color Color;
        public string Summary;
        public float Activation;
    }

    /// <summary>
    /// 一场战斗的数值状态（纯逻辑，可单测）。
    /// 规则：
    ///  - 玩家 100 血；敌人血量/攻击/间隔/护盾来自 EnemyDefinition；
    ///  - 普通音符不造成伤害，只按判定计分并累积 Buff 进度；
    ///  - Buff 占 N 个音符，全部判定后按平均权重结算效果；
    ///  - 护盾型敌人：护盾先吸收 Buff 伤害，破盾后打本体；
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

        private readonly Dictionary<string, BuffProgress> m_Buffs = new Dictionary<string, BuffProgress>();
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

        public IEnumerable<BuffProgress> Buffs
        {
            get { return m_Buffs.Values; }
        }

        public BuffActivation LastActivation { get; private set; }

        /// <summary>登记一个 Buff；required 是它需要判定的音符数。</summary>
        public void RegisterBuff(CardDefinition buff, int required)
        {
            if (buff != null)
            {
                RegisterBuff(buff.Id, buff, required);
            }
        }

        /// <summary>用唯一 key 登记（同一 Buff 在谱面里出现多次时，每个槽位一个实例）。</summary>
        public void RegisterBuff(string key, CardDefinition buff, int required)
        {
            if (buff == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            m_Buffs[key] = new BuffProgress
            {
                Id = key,
                DisplayName = buff.DisplayName,
                Color = buff.Accent,
                Required = Mathf.Max(1, required),
                Definition = buff,
            };
        }

        /// <summary>判定一个带 Buff 的音符；返回本次是否刚好让 Buff 完成结算。</summary>
        public bool ApplyBuffNote(string buffId, Judgement judgement)
        {
            BuffProgress progress;
            if (string.IsNullOrEmpty(buffId) || !m_Buffs.TryGetValue(buffId, out progress) || progress.Complete)
            {
                return false;
            }

            progress.Judged++;
            progress.Weight += WeightOf(judgement);
            if (!progress.Complete)
            {
                return false;
            }

            Activate(progress);
            return true;
        }

        public static float WeightOf(Judgement judgement)
        {
            switch (judgement)
            {
                case Judgement.Perfect: return 1f;
                case Judgement.Good: return 0.7f;
                case Judgement.Bad: return 0.4f;
                default: return 0f;
            }
        }

        private void Activate(BuffProgress progress)
        {
            float activation = progress.Activation;
            string summary;

            var damage = progress.Definition.Effect as DamageEffect;
            var heal = progress.Definition.Effect as HealEffect;

            if (damage != null)
            {
                int amount = Mathf.RoundToInt(damage.Damage * activation);
                ApplyDamage(amount);
                summary = "造成 " + amount + " 伤害";
            }
            else if (heal != null)
            {
                int amount = Mathf.RoundToInt(heal.Heal * activation);
                PlayerHp = Mathf.Min(DefaultPlayerHp, PlayerHp + amount);
                summary = "回复 " + amount + " 生命";
            }
            else
            {
                summary = "无效果";
            }

            LastActivation = new BuffActivation
            {
                DisplayName = progress.DisplayName,
                Color = progress.Color,
                Summary = summary,
                Activation = activation,
            };
        }

        /// <summary>只结算判定、计分与敌人攻击；伤害由 Buff 触发。</summary>
        public void ApplyJudgement(Judgement judgement)
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

        public void ApplyDamage(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            if (ShieldActive)
            {
                ShieldHp -= amount;
                if (ShieldHp < 0f)
                {
                    EnemyHp += ShieldHp;
                    ShieldHp = 0f;
                }
            }
            else
            {
                EnemyHp -= amount;
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
