using System.Collections.Generic;
using CardRhythm.Charts;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorCharts
{
    /// <summary>
    /// 从音频自动生成谱面草稿：能量包络找 onset → 对齐 1/8 格 → 按亮度分轨。
    /// 生成结果一定需要手工修，只作为起点。
    /// </summary>
    public class ChartAudioGeneratorWindow : EditorWindow
    {
        private const int Lanes = 4;
        private const int SlotsPerBar = 8;

        private AudioClip m_Clip;
        private float m_Bpm = 120f;
        private float m_Offset;
        private float m_MinInterval = 0.09f;
        private bool m_DetectHolds = true;
        private string m_EnemyId = "attacker";
        private string m_ChartId = "chart_auto";

        [MenuItem("Tools/卡牌音游/谱面/从音频生成谱面", false, 73)]
        public static void Open()
        {
            var window = GetWindow<ChartAudioGeneratorWindow>("音频生成谱面");
            window.minSize = new Vector2(520f, 420f);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("从音频生成谱面草稿", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            m_Clip = (AudioClip)EditorGUILayout.ObjectField("音频", m_Clip, typeof(AudioClip), false);
            m_Bpm = EditorGUILayout.FloatField("BPM", m_Bpm);
            m_Offset = EditorGUILayout.FloatField("起始偏移（秒）", m_Offset);
            m_MinInterval = EditorGUILayout.Slider("最小音符间隔（秒）", m_MinInterval, 0.05f, 0.4f);
            m_DetectHolds = EditorGUILayout.Toggle("检测长按", m_DetectHolds);
            m_EnemyId = EditorGUILayout.TextField("敌人 Id", m_EnemyId);
            m_ChartId = EditorGUILayout.TextField("谱面 Id", m_ChartId);

            EditorGUILayout.Space(6f);
            EditorGUILayout.HelpBox(
                "自动生成只能给出节奏点，轨道和长按需要人工调整。\n" +
                "音频需要能读取采样（PCM / Decompress On Load）。",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(m_Clip == null))
            {
                if (GUILayout.Button("分析并生成", GUILayout.Height(36f)))
                {
                    Generate();
                }
            }
        }

        private void Generate()
        {
            if (m_Clip == null)
            {
                return;
            }

            List<float> onsets = DetectOnsets(m_Clip, m_Offset, m_MinInterval);
            if (onsets == null)
            {
                EditorUtility.DisplayDialog("失败", "无法读取音频采样，请把音频导入设置改成 Decompress On Load。", "好");
                return;
            }

            if (onsets.Count == 0)
            {
                EditorUtility.DisplayDialog("失败", "没有检测到明显的节奏点。", "好");
                return;
            }

            float secondsPerSlot = 60f / Mathf.Max(30f, m_Bpm) * 0.5f;
            var notes = new List<ChartNote>();
            var used = new HashSet<int>();

            for (int i = 0; i < onsets.Count; i++)
            {
                int slotIndex = Mathf.RoundToInt(onsets[i] / secondsPerSlot);
                if (slotIndex < 0 || used.Contains(slotIndex))
                {
                    continue;
                }

                used.Add(slotIndex);

                int lane = BrightnessLane(m_Clip, onsets[i]);
                int length = 0;
                if (m_DetectHolds && i + 1 < onsets.Count && onsets[i + 1] - onsets[i] > 0.45f)
                {
                    length = Mathf.Clamp(Mathf.RoundToInt((onsets[i + 1] - onsets[i]) / secondsPerSlot), 2, 8);
                }

                notes.Add(new ChartNote
                {
                    Bar = slotIndex / SlotsPerBar,
                    Slot = slotIndex % SlotsPerBar,
                    Lane = lane,
                    LengthSlots = length,
                });
            }

            int bars = Mathf.Max(4, Mathf.CeilToInt((m_Clip.length - m_Offset) / secondsPerSlot / SlotsPerBar));
            AssignBuffSlots(notes);

            string path = "Assets/GameMain/Resources/Charts/" + m_ChartId + ".asset";
            var chart = AssetDatabase.LoadAssetAtPath<ChartDefinition>(path);
            if (chart == null)
            {
                chart = ScriptableObject.CreateInstance<ChartDefinition>();
                AssetDatabase.CreateAsset(chart, path);
            }

            chart.Id = m_ChartId;
            chart.EnemyId = m_EnemyId;
            chart.Bpm = m_Bpm;
            chart.Bars = bars;
            chart.SetNotes(notes);

            EditorUtility.SetDirty(chart);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[CHART-AUDIO] onsets=" + onsets.Count + " notes=" + notes.Count + " bars=" + bars + " -> " + path);
            EditorUtility.DisplayDialog("完成", "生成 " + notes.Count + " 个音符，保存到 " + path, "好");
        }

        /// <summary>能量包络 + 自适应阈值找 onset，返回秒数。</summary>
        public static List<float> DetectOnsets(AudioClip clip, float offset, float minInterval)
        {
            if (clip == null)
            {
                return null;
            }

            int channels = Mathf.Max(1, clip.channels);
            int sampleCount = clip.samples * channels;
            var data = new float[sampleCount];
            if (!clip.GetData(data, 0))
            {
                return null;
            }

            const int frame = 1024;
            const int hop = 512;
            var energy = new List<float>();
            for (int start = 0; start + frame <= clip.samples; start += hop)
            {
                float sum = 0f;
                for (int i = 0; i < frame; i++)
                {
                    for (int c = 0; c < channels; c++)
                    {
                        float v = data[(start + i) * channels + c];
                        sum += v * v;
                    }
                }

                energy.Add(Mathf.Sqrt(sum / (frame * channels)));
            }

            var onsets = new List<float>();
            float lastTime = -1f;
            float sampleRate = clip.frequency;
            for (int i = 2; i < energy.Count - 2; i++)
            {
                float window = 0f;
                int count = 0;
                for (int k = i - 12; k <= i + 12; k++)
                {
                    if (k >= 0 && k < energy.Count)
                    {
                        window += energy[k];
                        count++;
                    }
                }

                float threshold = window / Mathf.Max(1, count) * 1.6f + 0.008f;
                float current = energy[i];
                bool peak = current > threshold && current >= energy[i - 1] && current > energy[i + 1];
                float time = (i * hop + frame * 0.5f) / sampleRate - offset;
                if (peak && time > 0f && time - lastTime >= minInterval)
                {
                    onsets.Add(time);
                    lastTime = time;
                }
            }

            return onsets;
        }

        private static int BrightnessLane(AudioClip clip, float time)
        {
            // 用零交叉率粗分轨道：越亮越靠右。
            int channels = Mathf.Max(1, clip.channels);
            int start = Mathf.Clamp(Mathf.RoundToInt(time * clip.frequency), 0, Mathf.Max(0, clip.samples - 2048));
            var chunk = new float[2048 * channels];
            int available = Mathf.Min(chunk.Length, (clip.samples - start) * channels);
            if (available <= 0)
            {
                return 0;
            }

            if (!clip.GetData(chunk, start))
            {
                return 0;
            }

            int crossings = 0;
            for (int i = 1; i < available / channels; i++)
            {
                float previous = chunk[(i - 1) * channels];
                float current = chunk[i * channels];
                if ((previous < 0f && current >= 0f) || (previous >= 0f && current < 0f))
                {
                    crossings++;
                }
            }

            float rate = crossings / (float)(available / channels);
            int lane = Mathf.Clamp(Mathf.FloorToInt(rate * 12f), 0, Lanes - 1);
            return lane;
        }

        private static void AssignBuffSlots(List<ChartNote> notes)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                if (i % 16 != 0)
                {
                    continue;
                }

                string slotId = "S" + (i / 16 + 1);
                for (int k = i; k < notes.Count && k < i + 4; k++)
                {
                    ChartNote note = notes[k];
                    note.SlotId = slotId;
                    notes[k] = note;
                }
            }
        }
    }
}
