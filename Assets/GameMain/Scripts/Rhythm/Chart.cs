using System.Collections.Generic;

namespace CardRhythm.Rhythm
{
    /// <summary>一段可播放的谱面：BPM + 音符列表。</summary>
    public sealed class Chart
    {
        public readonly float Bpm;
        public readonly int Bars;
        public readonly int SlotsPerBar;
        public readonly List<NoteData> Notes;

        public Chart(float bpm, int bars, int slotsPerBar, List<NoteData> notes)
        {
            Bpm = bpm;
            Bars = bars;
            SlotsPerBar = slotsPerBar;
            Notes = notes;
            Notes.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        public float SecondsPerBeat
        {
            get { return 60f / Bpm; }
        }

        /// <summary>1/8 网格：每拍两格。</summary>
        public float SecondsPerSlot
        {
            get { return SecondsPerBeat * 0.5f; }
        }

        public float SlotTime(int bar, int slot)
        {
            return (bar * SlotsPerBar + slot) * SecondsPerSlot;
        }

        public float Duration
        {
            get { return Bars * SlotsPerBar * SecondsPerSlot; }
        }
    }
}
