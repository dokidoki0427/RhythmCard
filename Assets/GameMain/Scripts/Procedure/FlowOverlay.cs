using UnityEngine;

namespace CardRhythm.Procedure
{
    /// <summary>
    /// 骨架期的调试覆盖层：显示当前流程 + 操作提示。
    /// 正式 UI 做好后删掉，或改成仅 Development Build 显示。
    /// </summary>
    public class FlowOverlay : MonoBehaviour
    {
        private GUIStyle m_Style;

        private void OnGUI()
        {
            if (m_Style == null)
            {
                m_Style = new GUIStyle(GUI.skin.label);
                m_Style.fontSize = 16;
                m_Style.normal.textColor = new Color(1f, 1f, 1f, 0.45f);
            }

            GameProcedureBase current = GameProcedureBase.Current;
            string name = current != null ? current.GetType().Name : "(none)";
            GUI.Label(new Rect(Screen.width - 340f, 8f, 330f, 24f), name + "   [Space]", m_Style);
        }
    }
}
