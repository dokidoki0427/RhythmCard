using CardRhythm.Rhythm;
using NUnit.Framework;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>四个键的音效：音高必须四轨各不相同且从左到右升高。</summary>
    public class LaneAudioTests
    {
        [Test]
        public void Frequencies_AreAscending()
        {
            for (int lane = 1; lane < 4; lane++)
            {
                Assert.Greater(LaneAudioPlayer.FrequencyOf(lane), LaneAudioPlayer.FrequencyOf(lane - 1),
                    "第 " + lane + " 轨的音高必须高于前一轨");
            }
        }

        [Test]
        public void Frequencies_StayInAudibleRange()
        {
            for (int lane = 0; lane < 4; lane++)
            {
                float frequency = LaneAudioPlayer.FrequencyOf(lane);
                Assert.Greater(frequency, 200f);
                Assert.Less(frequency, 2000f);
            }
        }
    }
}
