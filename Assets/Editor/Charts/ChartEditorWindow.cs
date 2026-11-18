using System.Collections.Generic;
using System.Text;
using CardRhythm.Charts;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorCharts
{
    /// <summary>
    /// 谱面编辑器（最小可用）：选谱面 → 逐小节点击放置音符 → 给一组音符指定 Buff 槽位 → 保存。
    /// 槽位会在战斗开始时自动填入玩家持有的 Buff。
    /// </summary>
    public class ChartEditorWindow : EditorWindow
    {
        private const int Lanes = 4;
        private const int SlotsPerBar = 8;

        private List<ChartDefinition> m_Charts = new List<ChartDefinition>();
        private ChartDefinition m_Chart;
        private Vector2 m_Scroll;
        private string m_SlotId = "S1";
        private bool m_SlotMode;

        [MenuItem("Tools/卡牌音游/谱面编辑器", false, 72)]
        public static void Open()
        {
            var window = GetWindow<ChartEditorWindow>("谱面编辑器");
            window.minSize = new Vector2(900f, 620f);
            window.Reload();
        }

        private void OnEnable()
        {
            Reload();
        }

        private void Reload()
        {
            m_Charts.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:ChartDefinition", new[] { "Assets/GameMain/Resources/Charts" }))
            {
                var chart = AssetDatabase.LoadAssetAtPath<ChartDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (chart != null)
                {
                    m_Charts.Add(chart);
                }
            }

            m_Charts.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            if (m_Chart == null || !m_Charts.Contains(m_Chart))
            {
                m_Chart = m_Charts.Count > 0 ? m_Charts[0] : null;
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (m_Chart == null)
            {
                EditorGUILayout.HelpBox("没有谱面。先执行 Tools/卡牌音游/谱面/生成测试谱面。", MessageType.Info);
                return;
            }

            DrawChartHeader();
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            for (int bar = 0; bar < m_Chart.Bars; bar++)
            {
                DrawBar(bar);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    Reload();
                }

                EditorGUILayout.LabelField("谱面", GUILayout.Width(40f));
                int index = m_Charts.IndexOf(m_Chart);
                string[] names = new string[m_Charts.Count];
                for (int i = 0; i < m_Charts.Count; i++)
                {
                    names[i] = m_Charts[i].Id;
                }

                int next = EditorGUILayout.Popup(index, names, GUILayout.Width(220f));
                if (next != index && next >= 0 && next < m_Charts.Count)
                {
                    m_Chart = m_Charts[next];
                }

                GUILayout.FlexibleSpace();
                m_SlotMode = GUILayout.Toggle(m_SlotMode, "槽位模式", EditorStyles.toolbarButton, GUILayout.Width(80f));
                m_SlotId = EditorGUILayout.TextField(m_SlotId, GUILayout.Width(60f));
                if (GUILayout.Button("校验", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    Validate();
                }
            }
        }

        private void DrawChartHeader()
        {
            m_Chart.Id = EditorGUILayout.TextField("Id", m_Chart.Id);
            m_Chart.EnemyId = EditorGUILayout.TextField("敌人 Id", m_Chart.EnemyId);
            m_Chart.Bpm = EditorGUILayout.FloatField("BPM", m_Chart.Bpm);
            m_Chart.Bars = EditorGUILayout.IntSlider("小节数", m_Chart.Bars, 4, 64);
            EditorGUILayout.LabelField("提示：点亮格子放音符；槽位模式下放置的音符会归到当前槽位。");
        }

        private void DrawBar(int bar)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(bar.ToString("00"), GUILayout.Width(28f));
                for (int slot = 0; slot < SlotsPerBar; slot++)
                {
                    for (int lane = 0; lane < Lanes; lane++)
                    {
                        bool has = HasNote(bar, slot, lane);
                        var style = new GUIStyle(GUI.skin.button) { fontSize = 9 };
                        if (has)
                        {
                            style.normal.textColor = Color.black;
                        }

                        string label = has ? "●" : "·";
                        if (GUILayout.Button(label, style, GUILayout.Width(26f), GUILayout.Height(20f)))
                        {
                            ToggleNote(bar, slot, lane);
                        }
                    }

                    GUILayout.Space(4f);
                }
            }
        }

        private bool HasNote(int bar, int slot, int lane)
        {
            foreach (ChartNote note in m_Chart.Notes)
            {
                if (note.Bar == bar && note.Slot == slot && note.Lane == lane)
                {
                    return true;
                }
            }

            return false;
        }

        private void ToggleNote(int bar, int slot, int lane)
        {
            List<ChartNote> notes = m_Chart.Notes;
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].Bar == bar && notes[i].Slot == slot && notes[i].Lane == lane)
                {
                    notes.RemoveAt(i);
                    MarkDirty();
                    return;
                }
            }

            notes.Add(new ChartNote
            {
                Bar = bar,
                Slot = slot,
                Lane = lane,
                SlotId = m_SlotMode ? m_SlotId : string.Empty,
            });
            MarkDirty();
        }

        private void MarkDirty()
        {
            m_Chart.SetNotes(m_Chart.Notes);
            EditorUtility.SetDirty(m_Chart);
            AssetDatabase.SaveAssets();
        }

        private void Validate()
        {
            var report = new StringBuilder();
            var slots = m_Chart.SlotIds();
            report.AppendLine("谱面 " + m_Chart.Id + "：音符 " + m_Chart.Notes.Count + "，槽位 " + slots.Count);

            foreach (string slot in slots)
            {
                int count = m_Chart.SlotNoteCount(slot);
                report.AppendLine("  槽位 " + slot + " 音符数 " + count);
                if (count == 0)
                {
                    report.AppendLine("    警告：空槽位会浪费 Buff");
                }
            }

            if (slots.Count == 0)
            {
                report.AppendLine("  警告：没有 Buff 槽位，本场不会触发任何 Buff");
            }

            Debug.Log("[CHART-CHECK]\n" + report);
        }
    }
}
