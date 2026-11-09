using CardRhythm.Rhythm;
using NUnit.Framework;

namespace CardRhythm.Tests.EditMode
{
    /// <summary>
    /// 战斗视图的布局约束：三栏结构下，4 条轨道必须都在中间的轨道区内。
    /// </summary>
    public class StageLayoutTests
    {
        private const float SidePanelWidth = 380f;
        private const float ReferenceWidth = 1920f;

        [Test]
        public void LaneCenters_StayInsideMiddleArea()
        {
            for (int lane = 0; lane < 4; lane++)
            {
                float x = NoteStageView.LaneCenterX(lane);
                Assert.Greater(x, SidePanelWidth, "轨道中心不能落到左侧角色区");
                Assert.Less(x, ReferenceWidth - SidePanelWidth, "轨道中心不能落到右侧怪物区");
            }
        }

        [Test]
        public void LaneCenters_AreEvenlySpaced()
        {
            float firstGap = NoteStageView.LaneCenterX(1) - NoteStageView.LaneCenterX(0);
            for (int lane = 1; lane < 3; lane++)
            {
                float gap = NoteStageView.LaneCenterX(lane + 1) - NoteStageView.LaneCenterX(lane);
                Assert.AreEqual(firstGap, gap, 0.01f, "轨道间距必须一致");
            }
        }
    }
}
