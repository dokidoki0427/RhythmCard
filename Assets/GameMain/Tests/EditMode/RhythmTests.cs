using System.Collections.Generic;
using CardRhythm.Cards;
using CardRhythm.Rhythm;
using NUnit.Framework;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>
    /// 谱面生成与判定的回归测试。纯逻辑，不依赖场景/音频。
    /// </summary>
    public class RhythmTests
    {
        private static CardSpec Card(string id, int notes, CardPattern pattern, int seed = 0)
        {
            return new CardSpec(id, notes, pattern, seed, UnityEngine.Color.white);
        }

        private static List<CardSpec> Deck(params CardSpec[] cards)
        {
            return new List<CardSpec>(cards);
        }

        [Test]
        public void Chart_IsDeterministic()
        {
            var deck = Deck(Card("a", 1, CardPattern.Sequence), Card("b", 3, CardPattern.Mirror));
            Chart first = ChartGenerator.Generate(deck, 120f, 4);
            Chart second = ChartGenerator.Generate(deck, 120f, 4);

            Assert.AreEqual(first.Notes.Count, second.Notes.Count);
            for (int i = 0; i < first.Notes.Count; i++)
            {
                Assert.AreEqual(first.Notes[i].Time, second.Notes[i].Time, 0.0001f);
                Assert.AreEqual(first.Notes[i].Lane, second.Notes[i].Lane);
                Assert.AreEqual(first.Notes[i].SourceCardId, second.Notes[i].SourceCardId);
            }
        }

        [Test]
        public void Chart_UsesEightSlotGrid()
        {
            Chart chart = ChartGenerator.Generate(Deck(Card("a", 1, CardPattern.Sequence)), 120f, 1);
            // BPM120：一拍 0.5s，1/8 格 0.25s
            Assert.AreEqual(0.25f, chart.SecondsPerSlot, 0.0001f);
            Assert.AreEqual(0f, chart.Notes[0].Time, 0.0001f);
        }

        [Test]
        public void Chart_RespectsCardsPerBar()
        {
            var deck = Deck(Card("a", 1, CardPattern.Sequence), Card("b", 1, CardPattern.Sequence));
            Chart chart = ChartGenerator.Generate(deck, 120f, 4, cardsPerBar: 2, slotsPerBar: 8);

            // 每小节 2 张 1 音符的牌 = 2 个音符
            Assert.AreEqual(8, chart.Notes.Count);
            for (int bar = 0; bar < 4; bar++)
            {
                int inBar = 0;
                foreach (NoteData note in chart.Notes)
                {
                    if (note.Bar == bar)
                    {
                        inBar++;
                    }
                }

                Assert.AreEqual(2, inBar, "第 " + bar + " 小节音符数不对");
            }
        }

        [Test]
        public void Chart_QueuesOverflowToNextBar()
        {
            // 每小节最多 8 格，每张 4 音符；3 张/小节 必然溢出
            var deck = Deck(Card("a", 4, CardPattern.Sequence), Card("b", 4, CardPattern.Sequence));
            Chart chart = ChartGenerator.Generate(deck, 120f, 2, cardsPerBar: 3, slotsPerBar: 8);

            // 每小节实际只能放 2 张 = 8 个音符，第 3 张顺延
            foreach (NoteData note in chart.Notes)
            {
                Assert.Less(note.Slot, 8);
            }

            Assert.AreEqual(16, chart.Notes.Count);
        }

        [Test]
        public void Judger_PerfectWithin45Ms()
        {
            Chart chart = MakeChart(1.0f, 0);
            var judger = new Judger(chart);
            Assert.AreEqual(Judgement.Perfect, judger.Hit(0, 1.03f));
            Assert.AreEqual(1, judger.Judged);
        }

        [Test]
        public void Judger_GoodAndBadBoundaries()
        {
            var goodJudger = new Judger(MakeChart(1.0f));
            Assert.AreEqual(Judgement.Good, goodJudger.Hit(0, 1.06f));

            var badJudger = new Judger(MakeChart(1.0f));
            Assert.AreEqual(Judgement.Bad, badJudger.Hit(0, 1.12f));
        }

        [Test]
        public void Judger_MissAfterWindow()
        {
            Chart chart = MakeChart(1.0f);
            var judger = new Judger(chart);
            judger.Update(1.2f);

            Assert.AreEqual(1, judger.MissCount);
            Assert.AreEqual(0, judger.Combo);
            Assert.IsTrue(judger.IsFinished);
        }

        [Test]
        public void Judger_GhostTapIsIgnored()
        {
            Chart chart = MakeChart(1.0f);
            var judger = new Judger(chart);

            Assert.IsNull(judger.Hit(0, 0.2f), "空按必须返回 null，不惩罚");
            Assert.AreEqual(0, judger.Judged);
        }

        [Test]
        public void Judger_WrongLaneDoesNotConsumeNote()
        {
            Chart chart = MakeChart(1.0f);
            var judger = new Judger(chart);

            Assert.IsNull(judger.Hit(1, 1.0f));
            Assert.AreEqual(0, judger.Judged);
        }

        [Test]
        public void Judger_ComboBreaksOnMiss()
        {
            Chart chart = MakeChart(1.0f, 2.0f);
            var judger = new Judger(chart);

            judger.Hit(0, 1.0f);
            Assert.AreEqual(1, judger.Combo);

            judger.Update(2.2f);
            Assert.AreEqual(0, judger.Combo);
            Assert.AreEqual(1, judger.MaxCombo);
            Assert.AreEqual(1, judger.MissCount);
        }

        private static Chart MakeChart(params float[] times)
        {
            var notes = new List<NoteData>();
            for (int i = 0; i < times.Length; i++)
            {
                notes.Add(new NoteData(0, i, 0, times[i], "test"));
            }

            return new Chart(120f, 1, 8, notes);
        }
    }
}
