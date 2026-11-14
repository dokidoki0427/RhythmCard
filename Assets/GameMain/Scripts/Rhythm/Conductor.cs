using UnityEngine;

namespace CardRhythm.Rhythm
{
    /// <summary>
    /// 歌曲时钟。判定的唯一时间源是 AudioSettings.dspTime，不用 Time.time / deltaTime。
    /// 占位音乐：程序生成的 4 拍节拍器循环，用 PlayScheduled 对齐 dsp 时间。
    /// </summary>
    public class Conductor : MonoBehaviour
    {
        private AudioSource m_Source;
        private double m_StartDsp;
        private float m_Bpm = 120f;
        private bool m_Running;
        private bool m_Paused;
        private double m_PauseDsp;
        private float m_PausedSongTime;

        public bool IsRunning
        {
            get { return m_Running; }
        }

        public bool IsPaused
        {
            get { return m_Paused; }
        }

        public float Bpm
        {
            get { return m_Bpm; }
        }

        public float SecondsPerBeat
        {
            get { return 60f / m_Bpm; }
        }

        /// <summary>相对歌曲开始的时间（秒）。前奏期会是负数；暂停时冻结。</summary>
        public float SongTime
        {
            get
            {
                if (!m_Running)
                {
                    return 0f;
                }

                return m_Paused ? m_PausedSongTime : (float)(AudioSettings.dspTime - m_StartDsp);
            }
        }

        /// <summary>暂停：冻结时钟并暂停音频，不用 timeScale。</summary>
        public void Pause()
        {
            if (!m_Running || m_Paused)
            {
                return;
            }

            m_PauseDsp = AudioSettings.dspTime;
            m_PausedSongTime = (float)(m_PauseDsp - m_StartDsp);
            if (m_Source != null)
            {
                m_Source.Pause();
            }

            m_Paused = true;
        }

        /// <summary>恢复：把暂停时长补回起始 dsp，音频从原位置继续。</summary>
        public void Resume()
        {
            if (!m_Running || !m_Paused)
            {
                return;
            }

            m_StartDsp += AudioSettings.dspTime - m_PauseDsp;
            if (m_Source != null)
            {
                m_Source.UnPause();
            }

            m_Paused = false;
        }

        public void StartSong(float bpm, float leadInSeconds = 1f)
        {
            m_Bpm = bpm;
            if (m_Source == null)
            {
                m_Source = gameObject.AddComponent<AudioSource>();
                m_Source.playOnAwake = false;
                m_Source.loop = true;
                m_Source.volume = 0.3f;
            }

            m_Source.clip = CreateClickLoop(bpm);
            m_StartDsp = AudioSettings.dspTime + leadInSeconds;
            m_Source.PlayScheduled(m_StartDsp);
            m_Running = true;
        }

        public void StopSong()
        {
            m_Running = false;
            if (m_Source != null)
            {
                m_Source.Stop();
            }
        }

        /// <summary>生成 4 拍节拍器循环（设置页校准也复用）。</summary>
        public static AudioClip CreateClickLoop(float bpm)
        {
            const int sampleRate = 44100;
            float beat = 60f / bpm;
            int total = Mathf.RoundToInt(sampleRate * beat * 4f);
            var data = new float[total];

            for (int beatIndex = 0; beatIndex < 4; beatIndex++)
            {
                int start = Mathf.RoundToInt(sampleRate * beat * beatIndex);
                int length = Mathf.RoundToInt(sampleRate * 0.035f);
                float frequency = beatIndex == 0 ? 1600f : 1100f;

                for (int i = 0; i < length && start + i < total; i++)
                {
                    float envelope = 1f - i / (float)length;
                    data[start + i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * envelope * 0.5f;
                }
            }

            var clip = AudioClip.Create("metronome", total, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
