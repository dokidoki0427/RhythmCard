using UnityGameFramework.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 所有界面的基类。GF 的 UIFormLogic 已经提供了 OnInit / OnOpen / OnClose / OnPause / OnResume，
    /// 这里补：全屏拉伸（保证任何预制体根节点都铺满屏幕）、分组名、关闭自己。
    /// </summary>
    public abstract class UIFormBase : UIFormLogic
    {
        /// <summary>界面当前所在的 UI 组（即层级名）。</summary>
        public string GroupName
        {
            get
            {
                if (UIForm == null || UIForm.UIGroup == null)
                {
                    return string.Empty;
                }

                return UIForm.UIGroup.Name;
            }
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            StretchToScreen();
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            StretchToScreen();
            ApplyRuntimeFont();
        }

        /// <summary>预制体里的文字用内置字体（避免资源依赖），运行时统一换成系统中文字体。</summary>
        public void ApplyRuntimeFont()
        {
            foreach (Text text in GetComponentsInChildren<Text>(true))
            {
                text.font = SheetTheme.MainFont;
            }
        }

        /// <summary>
        /// 强制把界面根节点铺满父 Canvas：
        /// 不管预制体里的锚点/尺寸当初怎么摆，运行时都归一到全屏，避免界面偏到角落。
        /// </summary>
        public void StretchToScreen()
        {
            var rect = transform as RectTransform;
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        /// <summary>关闭自己。</summary>
        public void Close()
        {
            var uiComponent = GameEntry.GetComponent<UIComponent>();
            if (uiComponent == null || UIForm == null)
            {
                return;
            }

            uiComponent.CloseUIForm(UIForm);
        }
    }
}
