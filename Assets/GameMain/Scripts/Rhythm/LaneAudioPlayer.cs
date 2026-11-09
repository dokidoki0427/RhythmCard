using UnityEngine;

namespace CardRhythm.Rhythm
{
    /// <summary>
    /// 四个键的按键音效。占位阶段用程序合成的短音（正弦 + 二次谐波 + 指数衰减），
    /// 四轨音高依次升高（C5 D5 E5 G5，五声音阶），并按轨做左右声像，
    /// 这样不看屏幕也能听出自己按的是哪条轨。
    /// 正式音效接入时替换 CreateClip 即可。
    /// </summary>
    public class LaneAudioPlayer : MonoBehaviour
    {
        private static readonly float[] Frequencies = { 523.25f, 587.33f, 659.25f, 783.99f };
        private static AudioClip[] s_Clips;

        private AudioSource[] m_Sources;

        public static float FrequencyOf(int lane)
        {
            return Frequencies[Mathf.Clamp(lane, 0, Frequencies.Length - 1)];
        }

        public void Build()
        {
            EnsureClips();

            m_Sources = new AudioSource[Frequencies.Length];
            for (int lane = 0; lane < Frequencies.Length; lane++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.clip = s_Clips[lane];
                source.volume = 0.5f;
                source.panStereo = Mathf.Lerp(-0.45f, 0.45f, lane / 3f);
                m_Sources[lane] = source;
            }
        }

        public void Play(int lane)
        {
            if (m_Sources == null || lane < 0 || lane >= m_Sources.Length)
            {
                return;
            }

            m_Sources[lane].Play();
        }

        private static void EnsureClips()
        {
            if (s_Clips != null)
            {
                return;
            }

            s_Clips = new AudioClip[Frequencies.Length];
            for (int lane = 0; lane < Frequencies.Length; lane++)
            {
                s_Clips[lane] = CreateClip(Frequencies[lane], lane);
            }
        }

        private static AudioClip CreateClip(float frequency, int lane)
        {
            const int sampleRate = 44100;
            int length = Mathf.RoundToInt(sampleRate * 0.18f);
            var data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = i / (float)sampleRate;
                float attack = Mathf.Clamp01(t / 0.004f);
                float decay = Mathf.Exp(-t * 24f);
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t)
                             + 0.35f * Mathf.Sin(2f * Mathf.PI * frequency * 2f * t);

                data[i] = wave * attack * decay * 0.32f;
            }

            var clip = AudioClip.Create("lane_" + lane, length, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
