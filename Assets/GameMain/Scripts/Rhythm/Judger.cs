using System;
using System.Collections.Generic;

namespace CardRhythm.Rhythm
{
    public enum Judgement
    {
        Perfect = 0,
        Good = 1,
        Bad = 2,
        Miss = 3,
    }

    /// <summary>
    /// 判定器。纯逻辑，不依赖 Unity 时间——调用方把 dspTime 采样的秒数传进来。
    /// 已确认规则：
    ///  - 判定窗 Perfect 45ms / Good 90ms / Bad 135ms；
    ///  - 空按（附近没有音符）忽略，不惩罚；
    ///  - 超过窗口仍未按 = Miss；
    ///  - Miss 不吞音符（吞效果在战斗结算层处理）。
    /// </summary>
    public sealed class Judger
    {
        public float PerfectWindow = 0.045f;
        public float GoodWindow = 0.090f;
        public float BadWindow = 0.135f;

        private readonly Chart m_Chart;
        private readonly bool[] m_Judged;

        public Judger(Chart chart)
        {
            m_Chart = chart ?? throw new ArgumentNullException("chart");
            m_Judged = new bool[chart.Notes.Count];
            Total = chart.Notes.Count;
        }

        public int Total { get; private set; }
        public int Judged { get; private set; }
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }
        public int MissCount { get; private set; }
        public Judgement? LastJudgement { get; private set; }

        public bool IsFinished
        {
            get { return Judged >= Total; }
        }

        public event Action<NoteData, Judgement> JudgedEvent;

        /// <summary>玩家在 lane 轨按下（time = dspTime 秒）。返回判定结果；空按返回 null。</summary>
        public Judgement? Hit(int lane, float time)
        {
            for (int i = 0; i < m_Chart.Notes.Count; i++)
            {
                if (m_Judged[i])
                {
                    continue;
                }

                NoteData note = m_Chart.Notes[i];
                if (note.Time < time - BadWindow)
                {
                    continue; // 已经过了窗口，交给 Update 判 Miss
                }

                if (note.Time > time + BadWindow)
                {
                    break; // 音符按时间排序，后面的都太远
                }

                if (note.Lane != lane)
                {
                    continue;
                }

                return Judge(note, i, Math.Abs(time - note.Time));
            }

            return null;
        }

        /// <summary>每帧调用：把已经错过窗口的音符判成 Miss。</summary>
        public void Update(float songTime)
        {
            for (int i = 0; i < m_Chart.Notes.Count; i++)
            {
                if (m_Judged[i])
                {
                    continue;
                }

                NoteData note = m_Chart.Notes[i];
                if (songTime - note.Time > BadWindow)
                {
                    Judge(note, i, BadWindow + 1f);
                }
                else if (note.Time - songTime > BadWindow)
                {
                    break;
                }
            }
        }

        private Judgement Judge(NoteData note, int index, float delta)
        {
            Judgement result;
            if (delta <= PerfectWindow)
            {
                result = Judgement.Perfect;
            }
            else if (delta <= GoodWindow)
            {
                result = Judgement.Good;
            }
            else if (delta <= BadWindow)
            {
                result = Judgement.Bad;
            }
            else
            {
                result = Judgement.Miss;
            }

            m_Judged[index] = true;
            Judged++;

            if (result == Judgement.Miss)
            {
                Combo = 0;
                MissCount++;
            }
            else
            {
                Combo++;
                if (Combo > MaxCombo)
                {
                    MaxCombo = Combo;
                }
            }

            LastJudgement = result;
            var handler = JudgedEvent;
            if (handler != null)
            {
                handler(note, result);
            }

            return result;
        }
    }
}
