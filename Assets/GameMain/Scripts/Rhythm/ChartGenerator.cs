using System;
using System.Collections.Generic;
using CardRhythm.Cards;

namespace CardRhythm.Rhythm
{
    /// <summary>
    /// 牌组 → 谱面。纯函数：同样的输入永远得到同一份谱面。
    /// 规则（已确认）：
    ///  - 每小节固定出 cardsPerBar 张牌；
    ///  - 每张牌的音符落在连续的槽位上（1/8 网格，一小节 8 格）；
    ///  - 当前小节放不下时，这张牌顺延到下一小节先出。
    /// </summary>
    public static class ChartGenerator
    {
        public const int DefaultSlotsPerBar = 8;
        public const int DefaultCardsPerBar = 2;

        public static Chart Generate(IReadOnlyList<CardSpec> deck, float bpm, int bars,
            int cardsPerBar = DefaultCardsPerBar, int slotsPerBar = DefaultSlotsPerBar)
        {
            if (deck == null || deck.Count == 0)
            {
                throw new ArgumentException("牌组不能为空", "deck");
            }

            if (bpm <= 0f || bars <= 0 || cardsPerBar <= 0 || slotsPerBar <= 0)
            {
                throw new ArgumentOutOfRangeException("bpm/bars/cardsPerBar/slotsPerBar 必须为正");
            }

            float secondsPerSlot = 60f / bpm * 0.5f;
            var notes = new List<NoteData>(bars * cardsPerBar * 4);
            var pending = new Queue<CardSpec>();
            int deckIndex = 0;

            for (int bar = 0; bar < bars; bar++)
            {
                int slot = 0;
                int cardsThisBar = 0;

                while (cardsThisBar < cardsPerBar)
                {
                    CardSpec card = pending.Count > 0 ? pending.Dequeue() : deck[deckIndex++ % deck.Count];
                    int[] lanes = CardPatternUtility.Lanes(card.NoteCount, card.Pattern, card.EffectiveSeed);

                    if (slot + lanes.Length > slotsPerBar)
                    {
                        // 预算满：这张牌顺延到下一小节，本小节不再出牌。
                        pending.Enqueue(card);
                        break;
                    }

                    for (int i = 0; i < lanes.Length; i++)
                    {
                        float time = (bar * slotsPerBar + slot) * secondsPerSlot;
                        notes.Add(new NoteData(bar, slot, lanes[i], time, card.Id));
                        slot++;
                    }

                    cardsThisBar++;
                }
            }

            return new Chart(bpm, bars, slotsPerBar, notes);
        }
    }
}
