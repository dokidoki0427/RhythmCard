using System.Collections.Generic;
using CardRhythm.Cards;
using NUnit.Framework;
using UnityEngine;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>
    /// 卡牌数据与校验的回归测试。纯逻辑，不依赖编辑器窗口。
    /// </summary>
    public class CardTests
    {
        private readonly List<ScriptableObject> m_Created = new List<ScriptableObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (ScriptableObject asset in m_Created)
            {
                Object.DestroyImmediate(asset);
            }

            m_Created.Clear();
        }

        [Test]
        public void Lanes_LengthEqualsNoteCount()
        {
            for (int noteCount = 1; noteCount <= 4; noteCount++)
            {
                foreach (CardPattern pattern in System.Enum.GetValues(typeof(CardPattern)))
                {
                    int[] lanes = CardPatternUtility.Lanes(noteCount, pattern, 12345);
                    Assert.AreEqual(noteCount, lanes.Length, "轨道数必须等于音符数：" + noteCount + "/" + pattern);
                }
            }
        }

        [Test]
        public void Lanes_StayInRange()
        {
            for (int noteCount = 1; noteCount <= 4; noteCount++)
            {
                foreach (CardPattern pattern in System.Enum.GetValues(typeof(CardPattern)))
                {
                    foreach (int lane in CardPatternUtility.Lanes(noteCount, pattern, 7 * noteCount))
                    {
                        Assert.IsTrue(lane >= 0 && lane <= 3, "轨道越界：" + lane);
                    }
                }
            }
        }

        [Test]
        public void RandomPattern_IsDeterministic()
        {
            int[] first = CardPatternUtility.Lanes(4, CardPattern.Random, 42);
            int[] second = CardPatternUtility.Lanes(4, CardPattern.Random, 42);
            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void StableHash_DoesNotChangeBetweenCalls()
        {
            Assert.AreEqual(CardPatternUtility.StableHash("slash"), CardPatternUtility.StableHash("slash"));
            Assert.AreNotEqual(CardPatternUtility.StableHash("slash"), CardPatternUtility.StableHash("heavy_slash"));
            Assert.AreEqual(0, CardPatternUtility.StableHash(string.Empty));
        }

        [Test]
        public void Validator_AcceptsCleanCard()
        {
            CardDefinition card = MakeCard("slash", "斩击", 1);
            card.Effect = MakeDamageEffect(3);
            Assert.IsFalse(CardValidator.HasError(CardValidator.Validate(card, new[] { card })));
        }

        [Test]
        public void Validator_WarnsWhenEffectMissing()
        {
            CardDefinition card = MakeCard("no_effect", "空牌", 1);
            List<CardIssue> issues = CardValidator.Validate(card, new[] { card });
            Assert.IsFalse(CardValidator.HasError(issues), "缺效果只是警告");
            Assert.IsTrue(issues.Exists(i => i.Level == CardIssueLevel.Warning && i.Message.Contains("效果")));
        }

        [Test]
        public void Validator_RejectsBadId()
        {
            CardDefinition card = MakeCard("Slash!", "斩击", 1);
            card.Effect = MakeDamageEffect(3);
            List<CardIssue> issues = CardValidator.Validate(card, new[] { card });
            Assert.IsTrue(CardValidator.HasError(issues), "非法 Id 必须报错");
        }

        [Test]
        public void Validator_RejectsDuplicateId()
        {
            CardDefinition a = MakeCard("slash", "斩击", 1);
            CardDefinition b = MakeCard("slash", "斩击 2", 2);
            List<CardIssue> issues = CardValidator.Validate(a, new[] { a, b });
            Assert.IsTrue(CardValidator.HasError(issues), "Id 重复必须报错");
        }

        [Test]
        public void Validator_WarnsOnFlick()
        {
            CardDefinition card = MakeCard("flick_card", "滑动", 2);
            card.NoteType = CardNoteType.Flick;
            List<CardIssue> issues = CardValidator.Validate(card, new[] { card });
            Assert.IsFalse(CardValidator.HasError(issues), "Flick 只是警告，不是错误");
            Assert.IsTrue(issues.Exists(i => i.Level == CardIssueLevel.Warning && i.Message.Contains("Flick")));
        }

        [Test]
        public void DamageEffect_SummaryMatchesValue()
        {
            DamageEffect effect = MakeDamageEffect(12);
            Assert.AreEqual(12, effect.Damage);
            StringAssert.Contains("12", effect.Summary);
        }

        [Test]
        public void Card_DoesNotCarryDamage()
        {
            // 防止回退：卡牌数据上不应再有伤害字段。
            var property = typeof(CardDefinition).GetProperty("Damage");
            Assert.IsNull(property, "伤害应该在 CardEffect 上，而不是 CardDefinition");
        }

        private CardDefinition MakeCard(string id, string name, int noteCount)
        {
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            card.Id = id;
            card.DisplayName = name;
            card.NoteCount = noteCount;
            m_Created.Add(card);
            return card;
        }

        private DamageEffect MakeDamageEffect(int damage)
        {
            var effect = ScriptableObject.CreateInstance<DamageEffect>();
            effect.Damage = damage;
            m_Created.Add(effect);
            return effect;
        }
    }
}
