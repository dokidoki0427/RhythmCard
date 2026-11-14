using System.Collections.Generic;
using CardRhythm.Rhythm;
using CardRhythm.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 设置：判定 offset 校准。跟着节拍敲 8 次取中位数，写入 PlayerPrefs；
    /// 判定时 BattleRunner 会把输入时间减去这个 offset。
    /// </summary>
    public sealed class SettingsForm : UIFormBase
    {
        private const float CalibrationBpm = 120f;
        private const int RequiredTaps = 8;

        private readonly List<float> m_Taps = new List<float>();

        private Text m_OffsetLabel;
        private Text m_StatusLabel;
        private readonly Text[] m_KeyLabels = new Text[4];
        private int m_RebindingLane = -1;
        private AudioSource m_Audio;
        private bool m_Built;
        private bool m_Calibrating;
        private double m_StartDsp;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (!m_Built)
            {
                BuildUi();
                m_Built = true;
            }

            RefreshOffset();
            RefreshKeys();
            SetStatus("按 Esc 关闭");
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (m_RebindingLane >= 0)
                {
                    m_RebindingLane = -1;
                    SetStatus("已取消改键");
                    return;
                }

                Close();
                return;
            }

            if (m_RebindingLane >= 0)
            {
                CaptureRebind();
                return;
            }

            if (!m_Calibrating)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.F)
                || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K)
                || Input.GetKeyDown(KeyCode.Space))
            {
                CaptureTap();
            }
        }

        private void CaptureRebind()
        {
            int lane = m_RebindingLane;
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (!Input.GetKeyDown(key) || key == KeyCode.None)
                {
                    continue;
                }

                if (GameSettings.IsLaneKeyTaken(key, lane))
                {
                    SetStatus("这个键已经被其它轨占用");
                    return;
                }

                GameSettings.SetLaneKey(lane, key);
                m_RebindingLane = -1;
                RefreshKeys();
                SetStatus("轨 " + lane + " 已绑定 " + key);
                return;
            }
        }

        private void BeginRebind(int lane)
        {
            m_RebindingLane = lane;
            SetStatus("按任意键绑定轨 " + lane + "（Esc 取消）");
        }

        private void RefreshKeys()
        {
            for (int lane = 0; lane < m_KeyLabels.Length; lane++)
            {
                if (m_KeyLabels[lane] != null)
                {
                    m_KeyLabels[lane].text = GameSettings.GetLaneKey(lane).ToString();
                }
            }
        }

        private void BuildUi()
        {
            var panel = UIWidgets.Panel(transform, "SettingsPanel", new Vector2(920f, 620f), Vector2.zero,
                new Color(0.1f, 0.11f, 0.14f, 0.98f));

            UIWidgets.Label(panel, "SETTINGS", 44, TextAnchor.MiddleCenter, Color.white,
                new Vector2(800f, 60f), new Vector2(0f, 250f));

            m_OffsetLabel = UIWidgets.Label(panel, "OFFSET 0 ms", 32, TextAnchor.MiddleCenter,
                new Color(0.9f, 0.85f, 0.6f), new Vector2(800f, 50f), new Vector2(0f, 170f));

            UIWidgets.Button(panel, "-50", new Vector2(120f, 60f), new Vector2(-270f, 110f), () => AdjustOffset(-50), new Color(0.22f, 0.24f, 0.3f));
            UIWidgets.Button(panel, "-10", new Vector2(120f, 60f), new Vector2(-135f, 110f), () => AdjustOffset(-10), new Color(0.22f, 0.24f, 0.3f));
            UIWidgets.Button(panel, "+10", new Vector2(120f, 60f), new Vector2(0f, 110f), () => AdjustOffset(10), new Color(0.22f, 0.24f, 0.3f));
            UIWidgets.Button(panel, "+50", new Vector2(120f, 60f), new Vector2(135f, 110f), () => AdjustOffset(50), new Color(0.22f, 0.24f, 0.3f));
            UIWidgets.Button(panel, "0", new Vector2(120f, 60f), new Vector2(270f, 110f), () => SetOffset(0), new Color(0.3f, 0.2f, 0.2f));

            UIWidgets.Label(panel, "KEYS", 26, TextAnchor.MiddleCenter, new Color(0.75f, 0.75f, 0.75f),
                new Vector2(200f, 40f), new Vector2(0f, 55f));
            for (int lane = 0; lane < 4; lane++)
            {
                int captured = lane;
                float x = (lane - 1.5f) * 170f;
                UIWidgets.Button(panel, "轨 " + lane, new Vector2(150f, 58f), new Vector2(x, 5f), () => BeginRebind(captured), new Color(0.2f, 0.22f, 0.28f));
                m_KeyLabels[lane] = UIWidgets.Label(panel, "-", 24, TextAnchor.MiddleCenter, Color.white,
                    new Vector2(150f, 30f), new Vector2(x, -32f));
            }

            UIWidgets.Button(panel, "试听节拍", new Vector2(220f, 60f), new Vector2(-130f, -85f), PlayMetronome, new Color(0.18f, 0.28f, 0.36f));
            UIWidgets.Button(panel, "自动校准 (敲 8 次)", new Vector2(300f, 60f), new Vector2(150f, -85f), BeginCalibration, new Color(0.24f, 0.34f, 0.2f));

            m_StatusLabel = UIWidgets.Label(panel, string.Empty, 24, TextAnchor.MiddleCenter,
                new Color(0.8f, 0.8f, 0.8f), new Vector2(800f, 70f), new Vector2(0f, -150f));

            UIWidgets.Button(panel, "关闭", new Vector2(200f, 58f), new Vector2(0f, -235f), Close, new Color(0.3f, 0.3f, 0.34f));
        }

        private void AdjustOffset(int delta)
        {
            GameSettings.OffsetMs = GameSettings.OffsetMs + delta;
            RefreshOffset();
        }

        private void SetOffset(int value)
        {
            GameSettings.OffsetMs = value;
            RefreshOffset();
        }

        private void RefreshOffset()
        {
            if (m_OffsetLabel != null)
            {
                m_OffsetLabel.text = "OFFSET " + GameSettings.OffsetMs + " ms";
            }
        }

        private void PlayMetronome()
        {
            EnsureAudio();
            m_Audio.clip = Conductor.CreateClickLoop(CalibrationBpm);
            m_Audio.PlayScheduled(AudioSettings.dspTime + 0.2);
            SetStatus("听 4 拍：咚 哒 哒 哒");
        }

        private void BeginCalibration()
        {
            EnsureAudio();
            m_Taps.Clear();
            m_Calibrating = true;
            m_Audio.clip = Conductor.CreateClickLoop(CalibrationBpm);
            m_StartDsp = AudioSettings.dspTime + 0.6;
            m_Audio.PlayScheduled(m_StartDsp);
            SetStatus("跟着拍子按 D/F/J/K 或 Space（0/8）");
        }

        private void CaptureTap()
        {
            float t = (float)(AudioSettings.dspTime - m_StartDsp);
            if (t < 0.2f)
            {
                return;
            }

            m_Taps.Add(t);
            if (m_Taps.Count >= RequiredTaps)
            {
                FinishCalibration();
            }
            else
            {
                SetStatus("跟着拍子按 D/F/J/K 或 Space（" + m_Taps.Count + "/" + RequiredTaps + "）");
            }
        }

        private void FinishCalibration()
        {
            m_Calibrating = false;
            float beat = 60f / CalibrationBpm;
            var deltas = new List<float>(m_Taps.Count);
            foreach (float tap in m_Taps)
            {
                float nearestBeat = Mathf.Round(tap / beat) * beat;
                deltas.Add(tap - nearestBeat);
            }

            deltas.Sort();
            float median = deltas[deltas.Count / 2];
            int offsetMs = Mathf.RoundToInt(median * 1000f);
            GameSettings.OffsetMs = offsetMs;
            RefreshOffset();
            SetStatus("校准完成：offset " + offsetMs + " ms");
        }

        private void EnsureAudio()
        {
            if (m_Audio == null)
            {
                m_Audio = gameObject.AddComponent<AudioSource>();
                m_Audio.playOnAwake = false;
                m_Audio.loop = true;
                m_Audio.volume = 0.4f;
            }
        }

        private void SetStatus(string text)
        {
            if (m_StatusLabel != null)
            {
                m_StatusLabel.text = text;
            }
        }
    }
}
