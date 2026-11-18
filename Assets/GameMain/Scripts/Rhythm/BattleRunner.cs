using System;
using System.Collections.Generic;
using CardRhythm.Battle;
using CardRhythm.Cards;
using CardRhythm.Charts;
using CardRhythm.Settings;
using CardRhythm.UI;
using UnityEngine;

namespace CardRhythm.Rhythm
{
    /// <summary>
    /// 一场战斗的运行器：读取敌人固定谱面 → 把玩家持有的 Buff 填进槽位 → 起时钟 → 判定 → Buff 结算。
    /// 战斗不再有卡组与基础伤害。
    /// </summary>
    public class BattleRunner : MonoBehaviour
    {
        private const int MaxBuffs = 3;

        private Conductor m_Conductor;
        private NoteStageView m_View;
        private LaneAudioPlayer m_Audio;
        private BattleState m_State;
        private Dictionary<string, CardDefinition> m_BuffDefs = new Dictionary<string, CardDefinition>();
        private List<CardDefinition> m_Assigned = new List<CardDefinition>();
        private string m_EnemyName;
        private float m_Bpm;
        private float m_EndTime;

        public static BattleRunner Current { get; private set; }

        public event Action Finished;

        public bool IsFinished { get; private set; }
        public bool AbortRequested { get; private set; }
        public Judger Judger { get; private set; }
        public Chart Chart { get; private set; }

        public BattleState State
        {
            get { return m_State; }
        }

        public void Begin(float bpm = 120f, int bars = 32)
        {
            var enemy = LoadEnemy();
            m_EnemyName = enemy != null ? enemy.DisplayName : "未知敌人";
            m_State = new BattleState(enemy);

            ChartDefinition definition = LoadChart(enemy);
            m_Bpm = definition != null ? definition.Bpm : bpm;
            Chart = BuildChart(definition);

            m_Conductor = gameObject.AddComponent<Conductor>();
            m_View = gameObject.AddComponent<NoteStageView>();
            m_Audio = gameObject.AddComponent<LaneAudioPlayer>();
            m_Audio.Build();

            Judger = new Judger(Chart);
            Judger.JudgedEvent += OnJudged;
            m_View.Build(Chart, m_Conductor, BuildColors(), Judger, m_State, m_EnemyName);

            m_EndTime = Chart.Duration + 2f;
            m_Conductor.StartSong(m_Bpm, 1.5f);
            Current = this;
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }

            if (Judger != null)
            {
                Judger.JudgedEvent -= OnJudged;
            }
        }

        private void Update()
        {
            if (Judger == null || IsFinished || !m_Conductor.IsRunning)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape) && !UIRouter.HasModal)
            {
                TogglePause();
                return;
            }

            if (m_Conductor.IsPaused)
            {
                if (!UIRouter.HasModal)
                {
                    m_Conductor.Resume();
                }

                return;
            }

            float songTime = m_Conductor.SongTime;
            Judger.Update(songTime);
            m_State.TickBar(Mathf.FloorToInt(songTime / (m_Conductor.SecondsPerBeat * 4f)));

            for (int lane = 0; lane < 4; lane++)
            {
                if (InputKeys.LaneDown(lane))
                {
                    m_Audio.Play(lane);
                    Judger.Hit(lane, songTime - GameSettings.OffsetSeconds);
                }
            }

            if (Judger.IsFinished || songTime > m_EndTime || m_State.IsVictory || m_State.IsDefeat)
            {
                FinishBattle();
            }
        }

        /// <summary>Esc 暂停/继续：冻结 dspTime 与音频，不用 timeScale。</summary>
        private void TogglePause()
        {
            if (m_Conductor.IsPaused)
            {
                m_Conductor.Resume();
                UIRouter.Close(UIFormId.Pause);
            }
            else
            {
                m_Conductor.Pause();
                UIRouter.Open(UIFormId.Pause, null, true, true);
            }
        }

        /// <summary>暂停菜单里“退出到菜单”。</summary>
        public void RequestAbort()
        {
            AbortRequested = true;
            UIRouter.Close(UIFormId.Pause);
        }

        private void OnJudged(NoteData note, Judgement judgement)
        {
            if (judgement != Judgement.Miss)
            {
                m_View.FlashLane(note.Lane, NoteStageView.ColorOf(judgement));
            }

            m_State.ApplyJudgement(judgement);

            if (!string.IsNullOrEmpty(note.BuffId) && m_State.ApplyBuffNote(note.BuffId, judgement))
            {
                BuffActivation activation = m_State.LastActivation;
                if (activation != null)
                {
                    m_View.ShowPlayedCard(note.BuffId, activation.DisplayName, activation.Color,
                        Mathf.RoundToInt(activation.Activation * 100f), null);
                }
            }
        }

        private void FinishBattle()
        {
            IsFinished = true;
            m_Conductor.StopSong();
            BattleResult.Last = BattleResult.From(m_State, m_State.IsVictory);

            var handler = Finished;
            if (handler != null)
            {
                handler();
            }
        }

        // ── 谱面与 Buff ─────────────────────────────────────────

        private Chart BuildChart(ChartDefinition definition)
        {
            if (definition == null)
            {
                return new Chart(m_Bpm, 1, 8, new List<NoteData>());
            }

            List<CardDefinition> owned = LoadOwnedBuffs();
            List<string> slots = definition.SlotIds();

            var slotBuff = new Dictionary<string, string>();
            for (int i = 0; i < slots.Count && owned.Count > 0; i++)
            {
                CardDefinition buff = owned[i % owned.Count];
                string key = buff.Id + "#" + i;
                slotBuff[slots[i]] = key;
                int required = Mathf.Min(buff.NoteCount, definition.SlotNoteCount(slots[i]));
                m_State.RegisterBuff(key, buff, required);
                m_BuffDefs[key] = buff;
                m_Assigned.Add(buff);
            }

            var notes = new List<NoteData>(definition.Notes.Count);
            float secondsPerSlot = 60f / definition.Bpm * 0.5f;
            foreach (ChartNote note in definition.Notes)
            {
                string buffKey = null;
                if (!string.IsNullOrEmpty(note.SlotId))
                {
                    string key;
                    if (slotBuff.TryGetValue(note.SlotId, out key))
                    {
                        buffKey = key;
                    }
                }

                float time = (note.Bar * 8 + note.Slot) * secondsPerSlot;
                notes.Add(new NoteData(note.Bar, note.Slot, note.Lane, time, buffKey));
            }

            return new Chart(definition.Bpm, definition.Bars, 8, notes);
        }

        private static List<CardDefinition> LoadOwnedBuffs()
        {
            var result = new List<CardDefinition>();
            foreach (string id in GameSettings.LoadDeck())
            {
                if (result.Count >= MaxBuffs)
                {
                    break;
                }

                var buff = Resources.Load<CardDefinition>("Buffs/" + id);
                if (buff != null)
                {
                    result.Add(buff);
                }
            }

            if (result.Count == 0)
            {
                // 首次运行没有存档：自动发三个基础 Buff，保证能直接开打。
                var all = Resources.LoadAll<CardDefinition>("Buffs");
                System.Array.Sort(all, (a, b) => string.CompareOrdinal(a.Id, b.Id));
                var ids = new List<string>();
                for (int i = 0; i < all.Length && i < MaxBuffs; i++)
                {
                    result.Add(all[i]);
                    ids.Add(all[i].Id);
                }

                if (ids.Count > 0)
                {
                    GameSettings.SaveDeck(ids);
                }
            }

            return result;
        }

        private Dictionary<string, Color> BuildColors()
        {
            var colors = new Dictionary<string, Color>();
            foreach (KeyValuePair<string, CardDefinition> pair in m_BuffDefs)
            {
                colors[pair.Key] = pair.Value.Accent;
            }

            return colors;
        }

        private static ChartDefinition LoadChart(EnemyDefinition enemy)
        {
            string id = enemy != null && !string.IsNullOrEmpty(enemy.Id) ? enemy.Id : RunState.CurrentEnemyId;
            var definition = Resources.Load<ChartDefinition>("Charts/chart_" + id);
            if (definition == null)
            {
                Debug.LogWarning("[BATTLE] 找不到谱面 Resources/Charts/chart_" + id);
            }

            return definition;
        }

        private static EnemyDefinition LoadEnemy()
        {
            string id = RunState.CurrentEnemyId;
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var enemy = Resources.Load<EnemyDefinition>("Enemies/" + id);
            if (enemy == null)
            {
                Debug.LogWarning("[BATTLE] 找不到敌人模板 Resources/Enemies/" + id);
            }

            return enemy;
        }
    }
}
