using System.Collections.Generic;
using CardRhythm.Rhythm;
using UnityEngine;

namespace CardRhythm.Charts
{
    /// <summary>谱面里的一个音符。槽位 SlotId 为空表示普通音符。</summary>
    [System.Serializable]
    public struct ChartNote
    {
        public int Bar;
        public int Slot;
        public int Lane;
        public string SlotId;
    }

    /// <summary>
    /// 固定谱面。敌人绑定一张或多张；运行时由 BattleRunner 转成 <see cref="Chart"/>。
    /// 槽位（SlotId 相同的连续音符）会在开战时填入玩家持有的 Buff。
    /// </summary>
    [CreateAssetMenu(menuName = "CardRhythm/Chart", fileName = "chart_new")]
    public class ChartDefinition : ScriptableObject
    {
        [SerializeField] private string m_Id = "chart_new";
        [SerializeField] private string m_EnemyId = "attacker";
        [SerializeField] private float m_Bpm = 120f;
        [SerializeField] private int m_Bars = 16;
        [SerializeField] private List<ChartNote> m_Notes = new List<ChartNote>();

        public string Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        public string EnemyId
        {
            get { return m_EnemyId; }
            set { m_EnemyId = value; }
        }

        public float Bpm
        {
            get { return m_Bpm; }
            set { m_Bpm = Mathf.Max(30f, value); }
        }

        public int Bars
        {
            get { return m_Bars; }
            set { m_Bars = Mathf.Max(1, value); }
        }

        public List<ChartNote> Notes
        {
            get { return m_Notes; }
        }

        public void SetNotes(List<ChartNote> notes)
        {
            m_Notes = notes ?? new List<ChartNote>();
        }

        /// <summary>谱面里出现的槽位（去重，按出现顺序）。</summary>
        public List<string> SlotIds()
        {
            var ids = new List<string>();
            foreach (ChartNote note in m_Notes)
            {
                if (!string.IsNullOrEmpty(note.SlotId) && !ids.Contains(note.SlotId))
                {
                    ids.Add(note.SlotId);
                }
            }

            return ids;
        }

        /// <summary>某个槽位包含多少个音符。</summary>
        public int SlotNoteCount(string slotId)
        {
            int count = 0;
            foreach (ChartNote note in m_Notes)
            {
                if (note.SlotId == slotId)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
