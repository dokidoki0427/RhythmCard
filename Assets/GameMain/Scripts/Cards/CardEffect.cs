using UnityEngine;

namespace CardRhythm.Cards
{
    /// <summary>
    /// 卡牌功能基类。具体效果（伤害、治疗、抽牌…）各自派生一个 ScriptableObject，
    /// 由 CardDefinition.Effect 引用。这样“数值/规则”跟“卡牌身份与演出”解耦：
    /// 改伤害只动效果资产，改牌面只动卡牌资产。
    /// </summary>
    public abstract class CardEffect : ScriptableObject
    {
        /// <summary>给编辑器列表/卡面用的一句话摘要。</summary>
        public abstract string Summary { get; }
    }
}
