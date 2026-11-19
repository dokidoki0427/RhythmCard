using System.Collections.Generic;
using CardRhythm.Rhythm;
using NUnit.Framework;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>长按：按满结算成功，提前松开算失败。</summary>
    public class HoldTests
    {
        private static Chart MakeHoldChart()
        {
            var notes = new List<NoteData>
            {
                new NoteData(0, 0, 1, 0f, null, NoteType.Hold, 0.5f),
            };
            return new Chart(120f, 1, 8, notes);
        }

        [Test]
        public void Hold_CompletedWhenHeldToEnd()
        {
            var judger = new Judger(MakeHoldChart());
            bool? result = null;
            judger.HoldJudgedEvent += (note, completed) => result = completed;

            Assert.AreEqual(Judgement.Perfect, judger.Hit(1, 0f));
            judger.Update(0.3f, new[] { false, true, false, false });
            Assert.IsNull(result, "还没到尾，不应结算");

            judger.Update(0.6f, new[] { false, true, false, false });
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, judger.MissCount);
        }

        [Test]
        public void Hold_FailsWhenReleasedEarly()
        {
            var judger = new Judger(MakeHoldChart());
            bool? result = null;
            judger.HoldJudgedEvent += (note, completed) => result = completed;

            judger.Hit(1, 0f);
            judger.Update(0.2f, new[] { false, false, false, false });

            Assert.AreEqual(false, result);
            Assert.AreEqual(1, judger.MissCount);
            Assert.AreEqual(0, judger.Combo);
        }
    }
}
