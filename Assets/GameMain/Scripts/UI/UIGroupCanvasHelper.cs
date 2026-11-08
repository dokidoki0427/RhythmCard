using UnityGameFramework.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 界面组辅助器：给每个 UI 组建一个独立的 Screen Space - Overlay Canvas。
    /// GF 自带的 DefaultUIGroupHelper 是空实现（不建 Canvas），所以必须自定义一个，
    /// 否则界面实例没有 Canvas 祖先，运行时什么都不显示。
    /// 组的 depth 直接映射为 Canvas.sortingOrder，层级关系就靠它保证。
    /// </summary>
    public class UIGroupCanvasHelper : UIGroupHelperBase
    {
        private Canvas m_Canvas;
        private CanvasScaler m_Scaler;

        public override void SetDepth(int depth)
        {
            EnsureCanvas();
            m_Canvas.sortingOrder = depth;
        }

        private void EnsureCanvas()
        {
            if (m_Canvas != null)
            {
                return;
            }

            m_Canvas = gameObject.GetComponent<Canvas>();
            if (m_Canvas == null)
            {
                m_Canvas = gameObject.AddComponent<Canvas>();
            }

            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Canvas.pixelPerfect = false;

            m_Scaler = gameObject.GetComponent<CanvasScaler>();
            if (m_Scaler == null)
            {
                m_Scaler = gameObject.AddComponent<CanvasScaler>();
            }

            m_Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            m_Scaler.referenceResolution = new Vector2(1920f, 1080f);
            m_Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            m_Scaler.matchWidthOrHeight = 0.5f;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }
}
