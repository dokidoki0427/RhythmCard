using System.Collections.Generic;
using CardRhythm.Battle;
using CardRhythm.Rhythm;
using NUnit.Framework;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>战斗数值：伤害拆分、Miss 吞效果、强拍加成、敌人行动节奏、胜负。</summary>
    public class BattleStateTests
    {
        [Test]
        public void Perfect_DealsFullDamageAndScores()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Perfect, 10f, false);

            Assert.AreEqual(290f, state.EnemyHp, 0.001f);
            Assert.AreEqual(100, state.Score);
            Assert.AreEqual(1, state.Combo);
            Assert.AreEqual(1, state.Perfect);
        }

        [Test]
        public void Miss_DealsNoDamageButCounts()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Miss, 10f, false);

            Assert.AreEqual(300f, state.EnemyHp, 0.001f, "Miss 必须吞掉效果");
            Assert.AreEqual(0, state.Combo);
            Assert.AreEqual(1, state.Miss);
        }

        [Test]
        public void Accent_MultipliesDamage()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Perfect, 10f, true);

            Assert.AreEqual(285f, state.EnemyHp, 0.001f);
        }

        [Test]
        public void Enemy_AttacksEveryTwoNonPerfect()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Good, 0f, false);
            Assert.AreEqual(100f, state.PlayerHp, 0.001f);

            state.ApplyJudgement(Judgement.Bad, 0f, false);
            Assert.AreEqual(90f, state.PlayerHp, 0.001f);
        }

        [Test]
        public void Perfect_DoesNotTriggerEnemyAttack()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Perfect, 0f, false);
            state.ApplyJudgement(Judgement.Perfect, 0f, false);

            Assert.AreEqual(100f, state.PlayerHp, 0.001f);
        }

        [Test]
        public void Victory_WhenEnemyHpReachesZero()
        {
            var state = new BattleState { EnemyHp = 5f };
            state.ApplyJudgement(Judgement.Perfect, 10f, false);

            Assert.IsTrue(state.IsVictory);
        }

        [Test]
        public void Defeat_WhenPlayerHpReachesZero()
        {
            var state = new BattleState { PlayerHp = 10f };
            state.ApplyJudgement(Judgement.Miss, 0f, false);
            state.ApplyJudgement(Judgement.Miss, 0f, false);

            Assert.IsTrue(state.IsDefeat);
        }

        [Test]
        public void BattleResult_CopiesState()
        {
            var state = new BattleState();
            state.ApplyJudgement(Judgement.Perfect, 10f, false);
            state.ApplyJudgement(Judgement.Good, 5f, false);

            BattleResult result = BattleResult.From(state, true);
            Assert.IsTrue(result.Victory);
            Assert.AreEqual(state.Score, result.Score);
            Assert.AreEqual(state.MaxCombo, result.MaxCombo);
            Assert.AreEqual(state.Perfect, result.Perfect);
        }

        [Test]
        public void Shield_IgnoresNormalHits()
        {
            var state = new BattleState(240f, 12f, 2, CardRhythm.Cards.EnemyBehavior.Shielded, 80f, 8, 0);
            state.ApplyJudgement(Judgement.Perfect, 10f, false);

            Assert.AreEqual(240f, state.EnemyHp, 0.001f, "护盾未破时普通命中无效");
            Assert.AreEqual(80f, state.ShieldHp, 0.001f);
        }

        [Test]
        public void Shield_BreaksWithAccentAndOverflows()
        {
            var state = new BattleState(240f, 12f, 2, CardRhythm.Cards.EnemyBehavior.Shielded, 12f, 8, 0);
            state.ApplyJudgement(Judgement.Perfect, 10f, true);

            Assert.AreEqual(0f, state.ShieldHp, 0.001f);
            Assert.AreEqual(237f, state.EnemyHp, 0.001f, "护盾溢出伤害要打本体");
        }

        [Test]
        public void Disruptor_TicksOncePerBar()
        {
            var state = new BattleState(260f, 8f, 1, CardRhythm.Cards.EnemyBehavior.Disruptor, 0f, 8, 6);

            state.TickBar(8);
            state.TickBar(8);
            Assert.AreEqual(94f, state.PlayerHp, 0.001f, "同一小节只掉一次");

            state.TickBar(16);
            Assert.AreEqual(88f, state.PlayerHp, 0.001f);
        }
    }
}
