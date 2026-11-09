using System;
using System.Collections.Generic;
using CardRhythm.Cards;
using UnityEngine;

namespace CardRhythm.Rhythm
{
    /// <summary>
    /// 一场战斗的运行器：建牌组 → 生成谱面 → 启动时钟 → 读输入 → 判定 → 结束。
    /// 占位阶段牌组写死 8 张（4 种示例牌各 2 张），等卡组系统做好后替换 BuildDeck。
    /// </summary>
    public class BattleRunner : MonoBehaviour
    {
        private static readonly KeyCode[] LaneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };

        private Conductor m_Conductor;
        private NoteStageView m_View;
        private float m_Bpm;
        private float m_EndTime;

        public event Action Finished;

        public bool IsFinished { get; private set; }
        public Judger Judger { get; private set; }
        public Chart Chart { get; private set; }

        public void Begin(float bpm = 120f, int bars = 32)
        {
            m_Bpm = bpm;
            List<CardSpec> deck = BuildDeck();
            Chart = ChartGenerator.Generate(deck, bpm, bars);

            m_Conductor = gameObject.AddComponent<Conductor>();
            m_View = gameObject.AddComponent<NoteStageView>();
            Judger = new Judger(Chart);
            m_View.Build(Chart, m_Conductor, BuildColors(deck));

            m_EndTime = Chart.Duration + 2f;
            m_Conductor.StartSong(bpm, 1.5f);
        }

        private void Update()
        {
            if (Judger == null || IsFinished || !m_Conductor.IsRunning)
            {
                return;
            }

            float songTime = m_Conductor.SongTime;
            Judger.Update(songTime);

            for (int lane = 0; lane < LaneKeys.Length; lane++)
            {
                if (Input.GetKeyDown(LaneKeys[lane]))
                {
                    Judger.Hit(lane, songTime);
                }
            }

            if (Judger.IsFinished || songTime > m_EndTime)
            {
                IsFinished = true;
                m_Conductor.StopSong();
                var handler = Finished;
                if (handler != null)
                {
                    handler();
                }
            }
        }

        private void OnGUI()
        {
            if (Judger == null)
            {
                return;
            }

            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 22;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(24f, 96f, 600f, 28f), "Combo: " + Judger.Combo + "   Miss: " + Judger.MissCount, style);
            GUI.Label(new Rect(24f, 126f, 600f, 28f), "Judged: " + Judger.Judged + " / " + Judger.Total, style);
            if (Judger.LastJudgement.HasValue)
            {
                GUI.Label(new Rect(24f, 156f, 600f, 28f), "Last: " + Judger.LastJudgement.Value, style);
            }
        }

        private static List<CardSpec> BuildDeck()
        {
            var deck = new List<CardSpec>();
            for (int i = 0; i < 2; i++)
            {
                deck.Add(new CardSpec("slash", 1, CardPattern.Sequence, 0, new Color(0.35f, 0.75f, 1f)));
                deck.Add(new CardSpec("double_hit", 2, CardPattern.Alternate, 0, new Color(0.45f, 0.9f, 0.55f)));
                deck.Add(new CardSpec("heavy_slash", 3, CardPattern.Mirror, 0, new Color(1f, 0.72f, 0.3f)));
                deck.Add(new CardSpec("wild_strike", 4, CardPattern.Random, 7, new Color(0.95f, 0.45f, 0.85f)));
            }

            return deck;
        }

        private static Dictionary<string, Color> BuildColors(List<CardSpec> deck)
        {
            var colors = new Dictionary<string, Color>();
            foreach (CardSpec card in deck)
            {
                colors[card.Id] = card.Color;
            }

            return colors;
        }
    }
}
