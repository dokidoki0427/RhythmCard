using System.Collections.Generic;

namespace CardRhythm.UI
{
    /// <summary>界面 ID。资源路径与所属层级都集中在 <see cref="UIFormCatalog"/>。</summary>
    public enum UIFormId
    {
        Menu,
        Map,
        BattleHud,
        Result,
        Settings,
        Deck,
    }

    /// <summary>一个界面的静态定义：资源路径 + 它该落在哪一层。</summary>
    public readonly struct UIFormDef
    {
        public readonly UIFormId Id;
        public readonly string AssetPath;
        public readonly UILayer Layer;

        public UIFormDef(UIFormId id, string assetPath, UILayer layer)
        {
            Id = id;
            AssetPath = assetPath;
            Layer = layer;
        }
    }

    /// <summary>
    /// 界面目录。新增界面只改这里：加一个枚举值 + 一行定义，Router 和校验都自动生效。
    /// </summary>
    public static class UIFormCatalog
    {
        private const string UIFormRoot = "Assets/GameMain/UIForms/";

        private static readonly Dictionary<UIFormId, UIFormDef> s_Defs = new Dictionary<UIFormId, UIFormDef>
        {
            { UIFormId.Menu, new UIFormDef(UIFormId.Menu, UIFormRoot + "MenuForm.prefab", UILayer.Main) },
            { UIFormId.Map, new UIFormDef(UIFormId.Map, UIFormRoot + "MapForm.prefab", UILayer.Main) },
            { UIFormId.BattleHud, new UIFormDef(UIFormId.BattleHud, UIFormRoot + "BattleHudForm.prefab", UILayer.Hud) },
            { UIFormId.Result, new UIFormDef(UIFormId.Result, UIFormRoot + "ResultForm.prefab", UILayer.Window) },
            { UIFormId.Settings, new UIFormDef(UIFormId.Settings, UIFormRoot + "SettingsForm.prefab", UILayer.Popup) },
            { UIFormId.Deck, new UIFormDef(UIFormId.Deck, UIFormRoot + "DeckForm.prefab", UILayer.Popup) },
        };

        public static UIFormDef Get(UIFormId id)
        {
            return s_Defs[id];
        }

        public static IReadOnlyCollection<UIFormDef> All
        {
            get { return s_Defs.Values; }
        }
    }
}
