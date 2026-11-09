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
    }
}
