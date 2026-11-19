using System.Collections.Generic;
using System.IO;
using CardRhythm.Charts;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorCharts
{
    /// <summary>生成/重建每个敌人的测试谱面。后续可被手工谱面编辑器覆盖。</summary>
    public static class ChartAssetFactory
    {
        private const string Folder = "Assets/GameMain/Resources/Charts";

        private static readonly string[] Enemies = { "attacker", "disruptor", "shielded", "boss" };

        [MenuItem("Tools/卡牌音游/谱面/生成测试谱面", false, 70)]
        public static void GenerateMenu()
        {
            GenerateAll();
            EditorUtility.DisplayDialog("谱面", "已生成 4 张测试谱面。", "好");
        }

        public static void GenerateBatch()
        {
            GenerateAll();
            EditorApplication.Exit(0);
        }

        public static void GenerateAll()
        {
            EnsureFolder();
            foreach (string enemy in Enemies)
            {
                CreateOrUpdate(enemy);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CHART] generated");
        }

        private static void CreateOrUpdate(string enemyId)
        {
            string path = Folder + "/chart_" + enemyId + ".asset";
            var chart = AssetDatabase.LoadAssetAtPath<ChartDefinition>(path);
            if (chart == null)
            {
                chart = ScriptableObject.CreateInstance<ChartDefinition>();
                AssetDatabase.CreateAsset(chart, path);
            }

            chart.Id = "chart_" + enemyId;
            chart.EnemyId = enemyId;
            chart.Bpm = 120f;
            chart.Bars = 16;

            var notes = new List<ChartNote>();
            int seed = enemyId.GetHashCode();

            // 八个 Buff 槽位：奇数小节各 4 个音符，一个槽位正好对应一个 Buff。
            var slotBars = new[] { 1, 3, 5, 7, 9, 11, 13, 15 };
            for (int i = 0; i < slotBars.Length; i++)
            {
                AddSlot(notes, slotBars[i], "S" + (i + 1), seed + i);
            }

            // 普通填充音符：每小节 2 个，避开槽位小节。
            for (int bar = 0; bar < chart.Bars; bar++)
            {
                if (System.Array.IndexOf(slotBars, bar) >= 0)
                {
                    continue;
                }

                notes.Add(new ChartNote { Bar = bar, Slot = 0, Lane = (bar + seed) & 3 });
                notes.Add(new ChartNote
                {
                    Bar = bar,
                    Slot = 4,
                    Lane = (bar + seed + 2) & 3,
                    LengthSlots = bar % 4 == 0 ? 4 : 0,
                });
            }

            notes.Sort((a, b) => (a.Bar * 8 + a.Slot).CompareTo(b.Bar * 8 + b.Slot));
            chart.SetNotes(notes);
            EditorUtility.SetDirty(chart);
        }

        private static void AddSlot(List<ChartNote> notes, int bar, string slotId, int seed)
        {
            int[] lanes = { 0, 2, 1, 3 };
            for (int i = 0; i < 4; i++)
            {
                int slot = i * 2;
                int laneIndex = ((i + seed) % lanes.Length + lanes.Length) % lanes.Length;
                notes.Add(new ChartNote
                {
                    Bar = bar,
                    Slot = slot,
                    Lane = lanes[laneIndex],
                    SlotId = slotId,
                });
            }
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameMain/Resources/Charts"))
            {
                AssetDatabase.CreateFolder("Assets/GameMain/Resources", "Charts");
            }
        }
    }
}
