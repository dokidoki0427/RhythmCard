using System.Collections.Generic;

namespace CardRhythm.UI
{
    /// <summary>
    /// 界面层级。枚举值直接作为 GF 界面组的 depth，也作为 Canvas.sortingOrder。
    /// 数值留出间隔，方便以后在两层之间插新层。
    /// </summary>
    public enum UILayer
    {
        /// <summary>背景/场景底图，不可交互。</summary>
        Background = 0,

        /// <summary>主界面：菜单、地图等全屏页面。</summary>
        Main = 100,

        /// <summary>战斗 HUD：判定线、血量、combo。</summary>
        Hud = 200,

        /// <summary>窗口：结算、确认框等盖在主界面上的页面。</summary>
        Window = 300,

        /// <summary>弹窗：设置、暂停。</summary>
        Popup = 400,

        /// <summary>提示：toast、错误飘字，永远最上层。</summary>
        Tips = 500,
    }

    /// <summary>
    /// 层级工具。GF 的界面组名直接用枚举名，depth 用枚举值，避免两套命名。
    /// </summary>
    public static class UILayers
    {
        public static readonly IReadOnlyList<UILayer> All = new[]
        {
            UILayer.Background,
            UILayer.Main,
            UILayer.Hud,
            UILayer.Window,
            UILayer.Popup,
            UILayer.Tips,
        };

        public static string NameOf(UILayer layer)
        {
            return layer.ToString();
        }

        public static int DepthOf(UILayer layer)
        {
            return (int)layer;
        }
    }
}
