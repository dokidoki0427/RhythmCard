using UnityEngine;

namespace CardRhythm.Cards
{
    public enum CardNoteType
    {
        Tap = 0,
        Flick = 1,
    }

    /// <summary>一张牌在小节里的键位分布形状。</summary>
    public enum CardPattern
    {
        Sequence = 0,   // 从左到右依次
        Alternate = 1,  // 左右交替
        Mirror = 2,     // 由中间向两侧
        Random = 3,     // 按 seed 稳定随机
    }

    public enum CardRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
    }

    /// <summary>
    /// 卡牌数据。一个资产 = 一张牌。
    /// 这里只放“牌是什么、怎么落到音符上”，不放伤害/消耗这类战斗数值——
    /// 那些属于 <see cref="CardEffect"/>，由卡牌实际功能决定。
    /// Id 是稳定标识：存档、谱面 sourceCardId、掉落表都靠它，改显示名不要改 Id。
    /// </summary>
    [CreateAssetMenu(menuName = "CardRhythm/Card", fileName = "card_new")]
    public class CardDefinition : ScriptableObject
    {
        [SerializeField] private string m_Id = "card_new";
        [SerializeField] private string m_DisplayName = "新卡牌";
        [TextArea(2, 5)]
        [SerializeField] private string m_Description = string.Empty;
        [SerializeField] private CardRarity m_Rarity = CardRarity.Common;

        [Header("演出")]
        [SerializeField] private CardNoteType m_NoteType = CardNoteType.Tap;
        [SerializeField] private CardPattern m_Pattern = CardPattern.Sequence;
        [Range(1, 4)]
        [SerializeField] private int m_NoteCount = 1;
        [SerializeField] private int m_Seed = 0;

        [Header("表现")]
        [SerializeField] private Sprite m_Icon = null;
        [SerializeField] private Color m_Accent = new Color(0.35f, 0.62f, 1f, 1f);

        [Header("功能")]
        [SerializeField] private CardEffect m_Effect = null;

        public string Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        public string DisplayName
        {
            get { return m_DisplayName; }
            set { m_DisplayName = value; }
        }

        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        public CardRarity Rarity
        {
            get { return m_Rarity; }
            set { m_Rarity = value; }
        }

        public CardNoteType NoteType
        {
            get { return m_NoteType; }
            set { m_NoteType = value; }
        }

        public CardPattern Pattern
        {
            get { return m_Pattern; }
            set { m_Pattern = value; }
        }

        /// <summary>这张牌要打几个音符（1..4），只影响谱面生成。</summary>
        public int NoteCount
        {
            get { return m_NoteCount; }
            set { m_NoteCount = Mathf.Clamp(value, 1, 4); }
        }

        public int Seed
        {
            get { return m_Seed; }
            set { m_Seed = value; }
        }

        public Sprite Icon
        {
            get { return m_Icon; }
            set { m_Icon = value; }
        }

        public Color Accent
        {
            get { return m_Accent; }
            set { m_Accent = value; }
        }

        /// <summary>卡牌的实际功能。伤害、治疗、抽牌等都在这里，不在卡牌数据上。</summary>
        public CardEffect Effect
        {
            get { return m_Effect; }
            set { m_Effect = value; }
        }

        /// <summary>实际使用的随机种子：显式 seed 优先，否则用 Id 的稳定哈希。</summary>
        public int EffectiveSeed
        {
            get { return m_Seed != 0 ? m_Seed : CardPatternUtility.StableHash(m_Id); }
        }

        /// <summary>这张牌落在哪些轨道（0..3，对应 D F J K）。长度 = NoteCount。</summary>
        public int[] GetLanes()
        {
            return CardPatternUtility.Lanes(m_NoteCount, m_Pattern, EffectiveSeed);
        }

        private void OnValidate()
        {
            m_NoteCount = Mathf.Clamp(m_NoteCount, 1, 4);
            if (string.IsNullOrEmpty(m_Id))
            {
                m_Id = "card_" + Mathf.Abs(GetInstanceID());
            }
        }
    }
}
