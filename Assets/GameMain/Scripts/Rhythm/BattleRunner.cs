using System;
using System.Collections.Generic;
using System.Linq;
using CardRhythm.Battle;
using CardRhythm.Cards;
using CardRhythm.Settings;
using UnityEngine;

namespace CardRhythm.Rhythm
{
    /// <summary>
    /// 一场战斗的运行器：加载卡牌 → 建牌组 → 生成谱面 → 启动时钟 → 判定 → 数值结算 → 结束。
    /// 卡牌来自 Resources/Cards（原型阶段直接从 Resources 取，正式版换 GF 资源管线）。
    /// </summary>
    public class BattleRunner : MonoBehaviour
    {
        private static readonly KeyCode[] LaneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };
        private const int DeckSize = 8;
        private const int AccentCycleBars = 4;

        private Conductor m_Conductor;
        private NoteStageView m_View;
        private LaneAudioPlayer m_Audio;
        private BattleState m_State;
        private Dictionary<string, CardDefinition> m_Cards;
        private string m_AccentCardId;
        private string m_LastShownCardId;
        private float m_Bpm;
        private float m_EndTime;

        public event Action Finished;

        public bool IsFinished { get; private set; }
        public Judger Judger { get; private set; }
        public Chart Chart { get; private set; }
        public BattleState State
        {
            get { return m_State; }
        }

        public void Begin(float bpm = 120f, int bars = 32)
        {
            m_Bpm = bpm;
            m_Cards = LoadCards();
            List<CardDefinition> deckCards = ResolveDeckCards(m_Cards);
            List<CardSpec> deck = BuildDeck(deckCards);
            Chart = ChartGenerator.Generate(deck, bpm, bars);

            m_State = new BattleState(LoadEnemy());
            m_Conductor = gameObject.AddComponent<Conductor>();
            m_View = gameObject.AddComponent<NoteStageView>();
            m_Audio = gameObject.AddComponent<LaneAudioPlayer>();
            m_Audio.Build();

            Judger = new Judger(Chart);
            Judger.JudgedEvent += OnJudged;
            m_View.Build(Chart, m_Conductor, BuildColors(deck), Judger);

            m_EndTime = Chart.Duration + 2f;
            m_Conductor.StartSong(bpm, 1.5f);
        }

        private void OnDestroy()
        {
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

            float songTime = m_Conductor.SongTime;
            Judger.Update(songTime);
            m_State.TickBar(Mathf.FloorToInt(songTime / (m_Conductor.SecondsPerBeat * 4f)));

            for (int lane = 0; lane < LaneKeys.Length; lane++)
            {
                if (Input.GetKeyDown(LaneKeys[lane]))
                {
                    m_Audio.Play(lane);
                    Judger.Hit(lane, songTime - GameSettings.OffsetSeconds);
                }
            }

            UpdateAccent(songTime);

            if (Judger.IsFinished || songTime > m_EndTime || m_State.IsVictory || m_State.IsDefeat)
            {
                FinishBattle();
            }
        }

        private void UpdateAccent(float songTime)
        {
            if (!Input.GetKeyDown(KeyCode.Space))
            {
                return;
            }

            // 每 4 小节的最后 1 小节是强拍窗口。
            int bar = Mathf.FloorToInt(songTime / (m_Conductor.SecondsPerBeat * 4f));
            bool windowOpen = bar % AccentCycleBars == AccentCycleBars - 1;

            NoteData next;
            if (!windowOpen || !Judger.TryPeekNextNote(out next))
            {
                return;
            }

            m_AccentCardId = next.SourceCardId;
        }

        private void OnJudged(NoteData note, Judgement judgement)
        {
            if (note.SourceCardId != m_LastShownCardId)
            {
                m_LastShownCardId = note.SourceCardId;
                CardDefinition displayed;
                if (m_Cards != null && m_Cards.TryGetValue(note.SourceCardId, out displayed))
                {
                    var damageEffect = displayed.Effect as DamageEffect;
                    m_View.ShowPlayedCard(displayed.Id, displayed.DisplayName, displayed.Accent,
                        damageEffect != null ? damageEffect.Damage : 1, displayed.Icon);
                }
            }

            if (judgement != Judgement.Miss)
            {
                m_View.FlashLane(note.Lane, NoteStageView.ColorOf(judgement));
            }

            bool accent = !string.IsNullOrEmpty(m_AccentCardId) && note.SourceCardId == m_AccentCardId;
            m_State.ApplyJudgement(judgement, DamageOf(note), accent);
        }

        private float DamageOf(NoteData note)
        {
            CardDefinition card;
            if (m_Cards != null && m_Cards.TryGetValue(note.SourceCardId, out card))
            {
                var damageEffect = card.Effect as DamageEffect;
                if (damageEffect != null)
                {
                    return damageEffect.Damage / (float)Mathf.Max(1, card.NoteCount);
                }
            }

            return 1f;
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

        private void OnGUI()
        {
            if (m_State == null)
            {
                return;
            }

            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 22;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(24f, 96f, 700f, 28f), "Layer " + RunState.Layer + " / " + RunState.MaxLayer + "   " + m_State.Behavior, style);
            GUI.Label(new Rect(24f, 126f, 700f, 28f), "Player HP: " + m_State.DisplayPlayerHp + " / 100", style);
            GUI.Label(new Rect(24f, 156f, 700f, 28f), "Enemy HP: " + m_State.DisplayEnemyHp + " / " + Mathf.CeilToInt(m_State.MaxEnemyHp), style);
            if (m_State.Behavior == EnemyBehavior.Shielded)
            {
                GUI.Label(new Rect(24f, 186f, 700f, 28f), "Shield: " + m_State.DisplayShieldHp + "  (only accent breaks it)", style);
            }

            GUI.Label(new Rect(24f, 216f, 700f, 28f), "Combo: " + Judger.Combo + "   Score: " + m_State.Score, style);
            GUI.Label(new Rect(24f, 246f, 700f, 28f), "Judged: " + Judger.Judged + " / " + Judger.Total + "   Miss: " + m_State.Miss, style);

            if (!string.IsNullOrEmpty(m_AccentCardId))
            {
                GUI.Label(new Rect(24f, 276f, 700f, 28f), "ACCENT armed: " + m_AccentCardId, style);
            }
        }

        private static Dictionary<string, CardDefinition> LoadCards()
        {
            var cards = new Dictionary<string, CardDefinition>();
            foreach (CardDefinition card in Resources.LoadAll<CardDefinition>("Cards"))
            {
                if (card != null && !cards.ContainsKey(card.Id))
                {
                    cards.Add(card.Id, card);
                }
            }

            if (cards.Count == 0)
            {
                Debug.LogWarning("[BATTLE] Resources/Cards 为空，使用无效果占位牌。");
            }

            return cards;
        }

        private static List<CardDefinition> ResolveDeckCards(Dictionary<string, CardDefinition> all)
        {
            var result = new List<CardDefinition>();
            foreach (string id in GameSettings.LoadDeck())
            {
                CardDefinition card;
                if (all.TryGetValue(id, out card))
                {
                    result.Add(card);
                }
            }

            if (result.Count == 0)
            {
                result.AddRange(all.Values);
            }

            return result;
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

        private static List<CardSpec> BuildDeck(List<CardDefinition> cards)
        {
            if (cards.Count == 0)
            {
                var fallback = new List<CardSpec>(DeckSize);
                for (int i = 0; i < DeckSize; i++)
                {
                    fallback.Add(new CardSpec("placeholder", 1, CardPattern.Sequence, 0, Color.white));
                }

                return fallback;
            }

            var deck = new List<CardSpec>(cards.Count);
            foreach (CardDefinition card in cards)
            {
                deck.Add(new CardSpec(card.Id, Mathf.Clamp(card.NoteCount, 1, 4), card.Pattern, card.EffectiveSeed, card.Accent));
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
