using UnityGameFramework.Runtime;
using UnityEngine;

namespace CardRhythm.UI
{
    /// <summary>
    /// 所有界面的基类。GF 的 UIFormLogic 已经提供了 OnInit / OnOpen / OnClose / OnPause / OnResume，
    /// 这里只补两件业务层常用的事：拿到自己所在的分组名（层级），以及一个关闭自己的快捷方法。
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
