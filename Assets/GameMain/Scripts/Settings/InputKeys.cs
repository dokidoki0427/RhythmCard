using UnityEngine;

namespace CardRhythm.Settings
{
    /// <summary>四个轨道的按键读取（统一入口，方便重绑）。</summary>
    public static class InputKeys
    {
        public static KeyCode Lane(int lane)
        {
            return GameSettings.GetLaneKey(lane);
        }

        public static bool LaneDown(int lane)
        {
            return Input.GetKeyDown(Lane(lane));
        }

        public static bool LaneHeld(int lane)
        {
            return Input.GetKey(Lane(lane));
        }

        public static string LaneLabel(int lane)
        {
            return Lane(lane).ToString();
        }
    }
}
