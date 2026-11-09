using UnityEngine;

namespace CardRhythm.Rhythm
{
    /// <summary>一个音符。Time 是相对歌曲开始的时间（秒）。</summary>
    public struct NoteData
    {
        public int Bar;
        public int Slot;
        public int Lane;
        public float Time;
        public string SourceCardId;

        public NoteData(int bar, int slot, int lane, float time, string sourceCardId)
        {
            Bar = bar;
            Slot = slot;
            Lane = lane;
            Time = time;
            SourceCardId = sourceCardId;
        }
    }

    /// <summary>
    /// 谱面生成用的卡牌快照。故意不依赖 CardDefinition 资产，
    /// 这样生成器可以单测，也方便以后从存档/网络恢复。
    /// </summary>
    public readonly struct CardSpec
    {
        public readonly string Id;
        public readonly int NoteCount;
        public readonly Cards.CardPattern Pattern;
        public readonly int Seed;
        public readonly Color Color;

        public CardSpec(string id, int noteCount, Cards.CardPattern pattern, int seed, Color color)
        {
            Id = id;
            NoteCount = noteCount;
            Pattern = pattern;
            Seed = seed;
            Color = color;
        }

        public int EffectiveSeed
        {
            get { return Seed != 0 ? Seed : Cards.CardPatternUtility.StableHash(Id); }
        }
    }
}
