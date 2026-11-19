using CardRhythm.Battle;
using CardRhythm.Cards;
using CardRhythm.Rhythm;
using NUnit.Framework;
using UnityEngine;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>战斗数值：判定只计分、Buff 按判定权重结算、敌人行动节奏、胜负。</summary>
    public class BattleStateTests
    {
        private static CardDefinition MakeDamageBuff(string id, int damage)
        {
            var effect = ScriptableObject.CreateInstance<DamageEffect>();
            effect.Damage = damage;
            var buff = ScriptableObject.CreateInstance<CardDefinition>();
            buff.Id = id;
            buff.DisplayName = id;
            buff.Effect = effect;
            return buff;
        }

        private static CardDefinition MakeArmorBuff(string id, int armor)
        {
            var effect = ScriptableObject.CreateInstance<ArmorEffect>();
            effect.Armor = armor;
            var buff = ScriptableObject.CreateInstance<CardDefinition>();
            buff.Id = id;
            buff.DisplayName = id;
            buff.BuffType = BuffType.Defense;
            buff.Effect = effect;
            return buff;
        }

        [Test]
        public void Perfect_ScoresButDealsNoDamage()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Perfect);

            Assert.AreEqual(300f, state.EnemyHp, 0.001f, "普通音符不造成伤害");
            Assert.AreEqual(100, state.Score);
            Assert.AreEqual(1, state.Combo);
        }

        [Test]
        public void Miss_CountsAndKeepsHp()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Miss);

            Assert.AreEqual(300f, state.EnemyHp, 0.001f);
            Assert.AreEqual(0, state.Combo);
            Assert.AreEqual(1, state.Miss);
        }

        [Test]
        public void Buff_PerfectDealsFullDamage()
        {
            var state = new BattleState();
            state.RegisterBuff(MakeDamageBuff("buff_a", 10), 1);
            state.ApplyJudgement(Judgement.Perfect);
            bool activated = state.ApplyBuffNote("buff_a", Judgement.Perfect);

            Assert.IsTrue(activated);
            Assert.AreEqual(290f, state.EnemyHp, 0.001f);
        }

        [Test]
        public void Buff_ScalesWithJudgementWeights()
        {
            var state = new BattleState();
            state.RegisterBuff(MakeDamageBuff("buff_b", 20), 2);
            state.ApplyBuffNote("buff_b", Judgement.Perfect);
            state.ApplyBuffNote("buff_b", Judgement.Good);

            // (1.0 + 0.7) / 2 = 0.85 → 20 × 0.85 = 17
            Assert.AreEqual(283f, state.EnemyHp, 0.001f);
        }

        [Test]
        public void Buff_MissReducesActivation()
        {
            var state = new BattleState();
            state.RegisterBuff(MakeDamageBuff("buff_c", 10), 2);
            state.ApplyBuffNote("buff_c", Judgement.Perfect);
            state.ApplyBuffNote("buff_c", Judgement.Miss);

            // (1.0 + 0) / 2 = 0.5 → 5
            Assert.AreEqual(295f, state.EnemyHp, 0.001f);
        }

        [Test]
        public void Armor_AbsorbsEnemyAttackFirst()
        {
            var state = new BattleState();
            state.RegisterBuff(MakeArmorBuff("buff_armor", 15), 1);
            state.ApplyBuffNote("buff_armor", Judgement.Perfect);
            Assert.AreEqual(15f, state.PlayerArmor, 0.001f);

            state.ApplyJudgement(Judgement.Good);
            state.ApplyJudgement(Judgement.Bad);

            Assert.AreEqual(100f, state.PlayerHp, 0.001f, "护甲先挡伤害");
            Assert.AreEqual(5f, state.PlayerArmor, 0.001f);
        }

        [Test]
        public void Armor_OverflowDamagesHp()
        {
            var state = new BattleState();
            state.RegisterBuff(MakeArmorBuff("buff_armor2", 5), 1);
            state.ApplyBuffNote("buff_armor2", Judgement.Perfect);

            state.ApplyJudgement(Judgement.Good);
            state.ApplyJudgement(Judgement.Bad);

            Assert.AreEqual(0f, state.PlayerArmor, 0.001f);
            Assert.AreEqual(95f, state.PlayerHp, 0.001f);
        }

        [Test]
        public void Enemy_AttacksEveryTwoNonPerfect()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Good);
            Assert.AreEqual(100f, state.PlayerHp, 0.001f);

            state.ApplyJudgement(Judgement.Bad);
            Assert.AreEqual(90f, state.PlayerHp, 0.001f);
        }

        [Test]
        public void Perfect_DoesNotTriggerEnemyAttack()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Perfect);
            state.ApplyJudgement(Judgement.Perfect);

            Assert.AreEqual(100f, state.PlayerHp, 0.001f);
        }

        [Test]
        public void Shield_AbsorbsBuffDamage()
        {
            var state = new BattleState(240f, 12f, 2, EnemyBehavior.Shielded, 12f, 8, 0);
            state.ApplyDamage(10f);
            Assert.AreEqual(2f, state.ShieldHp, 0.001f);
            Assert.AreEqual(240f, state.EnemyHp, 0.001f);

            state.ApplyDamage(10f);
            Assert.AreEqual(0f, state.ShieldHp, 0.001f);
            Assert.AreEqual(232f, state.EnemyHp, 0.001f, "护盾溢出伤害打本体");
        }

        [Test]
        public void Victory_WhenEnemyHpReachesZero()
        {
            var state = new BattleState { EnemyHp = 5f };
            state.ApplyJudgement(Judgement.Perfect);

            Assert.IsFalse(state.IsVictory);

            state.ApplyDamage(5f);
            Assert.IsTrue(state.IsVictory);
        }

        [Test]
        public void Defeat_WhenPlayerHpReachesZero()
        {
            var state = new BattleState { PlayerHp = 10f };
            state.ApplyJudgement(Judgement.Miss);
            state.ApplyJudgement(Judgement.Miss);

            Assert.IsTrue(state.IsDefeat);
        }

        [Test]
        public void BattleResult_CopiesState()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Perfect);
            state.ApplyJudgement(Judgement.Good);

            BattleResult result = BattleResult.From(state, true);
            Assert.IsTrue(result.Victory);
            Assert.AreEqual(state.Score, result.Score);
            Assert.AreEqual(state.MaxCombo, result.MaxCombo);
            Assert.AreEqual(state.Perfect, result.Perfect);
        }

        [Test]
        public void Disruptor_TicksOncePerBar()
        {
            var state = new BattleState(260f, 8f, 1, EnemyBehavior.Disruptor, 0f, 8, 6);

            state.TickBar(8);
            state.TickBar(8);
            Assert.AreEqual(94f, state.PlayerHp, 0.001f, "同一小节只掉一次");

            state.TickBar(16);
            Assert.AreEqual(88f, state.PlayerHp, 0.001f);
        }
    }
}
