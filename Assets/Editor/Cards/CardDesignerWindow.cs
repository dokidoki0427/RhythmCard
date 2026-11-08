using System.Collections.Generic;
using System.Linq;
using CardRhythm.Cards;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorCards
{
    /// <summary>
    /// 卡牌制作器：左边列表、右边编辑 + 校验 + 键位预览。
    /// 只负责“牌是什么、怎么落到音符、挂什么功能”，战斗数值在效果资产里。
    /// 菜单：Tools/卡牌音游/卡牌编辑器
    /// </summary>
    public class CardDesignerWindow : EditorWindow
    {
        private const int LaneCount = 4;
        private const int PreviewColumns = 8;

        private List<CardDefinition> m_Cards = new List<CardDefinition>();
        private CardDefinition m_Selected;
        private SerializedObject m_Serialized;
        private Vector2 m_ListScroll;
        private Vector2 m_DetailScroll;
        private string m_Search = string.Empty;

        [MenuItem("Tools/卡牌音游/卡牌编辑器", false, 100)]
        public static void Open()
        {
            var window = GetWindow<CardDesignerWindow>("卡牌编辑器");
            window.minSize = new Vector2(880f, 560f);
            window.Reload();
        }

        private void OnEnable()
        {
            Reload();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.BeginHorizontal();
            DrawList();
            DrawDetail();
            EditorGUILayout.EndHorizontal();
        }

        private void Reload()
        {
            m_Cards = CardAssetFactory.LoadAll();
            if (m_Selected == null || !m_Cards.Contains(m_Selected))
            {
                m_Selected = m_Cards.FirstOrDefault();
            }

            RebuildSerialized();
            Repaint();
        }

        private void Select(CardDefinition card)
        {
            m_Selected = card;
            RebuildSerialized();
            Selection.activeObject = card;
            Repaint();
        }

        private void RebuildSerialized()
        {
            m_Serialized = m_Selected != null ? new SerializedObject(m_Selected) : null;
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("新建卡牌", EditorStyles.toolbarButton, GUILayout.Width(80f)))
                {
                    var created = CardAssetFactory.Create("新卡牌");
                    Reload();
                    Select(created);
                    GUIUtility.ExitGUI();
                }

                using (new EditorGUI.DisabledScope(m_Selected == null))
                {
                    if (GUILayout.Button("复制", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                    {
                        var copy = CardAssetFactory.Duplicate(m_Selected);
                        Reload();
                        Select(copy);
                        GUIUtility.ExitGUI();
                    }

                    if (GUILayout.Button("删除", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                    {
                        if (EditorUtility.DisplayDialog("删除卡牌", "确定删除 " + m_Selected.DisplayName + " ?", "删除", "取消"))
                        {
                            CardAssetFactory.Delete(m_Selected);
                            m_Selected = null;
                            Reload();
                            GUIUtility.ExitGUI();
                        }
                    }
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    Reload();
                }

                m_Search = GUILayout.TextField(m_Search ?? string.Empty, EditorStyles.toolbarSearchField, GUILayout.Width(220f));
            }
        }

        private void DrawList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(260f)))
            {
                EditorGUILayout.LabelField("卡牌（" + m_Cards.Count + "）", EditorStyles.boldLabel);

                m_ListScroll = EditorGUILayout.BeginScrollView(m_ListScroll, EditorStyles.helpBox);
                foreach (CardDefinition card in m_Cards)
                {
                    if (card == null || !Matches(card))
                    {
                        continue;
                    }

                    string effect = card.Effect != null ? card.Effect.Summary : "无效果";
                    string label = card.DisplayName + "\n" + card.Id + "   " + card.NoteCount + " 音  ·  " + effect;
                    var style = card == m_Selected ? EditorStyles.helpBox : EditorStyles.label;
                    if (GUILayout.Button(label, style, GUILayout.Height(46f)))
                    {
                        Select(card);
                        GUIUtility.ExitGUI();
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawDetail()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (m_Selected == null)
                {
                    EditorGUILayout.HelpBox("还没有卡牌。点左上角“新建卡牌”。", MessageType.Info);
                    return;
                }

                m_DetailScroll = EditorGUILayout.BeginScrollView(m_DetailScroll);

                DrawAssetInfo();
                DrawFields();
                DrawEffect();
                DrawValidation();
                DrawPreview();

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawAssetInfo()
        {
            string path = AssetDatabase.GetAssetPath(m_Selected);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(path, EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("定位资源", EditorStyles.miniButton, GUILayout.Width(80f)))
                {
                    EditorGUIUtility.PingObject(m_Selected);
                }

                if (GUILayout.Button("文件改名 = Id", EditorStyles.miniButton, GUILayout.Width(110f)))
                {
                    string error = CardAssetFactory.RenameAssetToId(m_Selected);
                    if (!string.IsNullOrEmpty(error))
                    {
                        EditorUtility.DisplayDialog("改名失败", error, "好");
                    }

                    Reload();
                }
            }
        }

        private void DrawFields()
        {
            m_Serialized.Update();

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("基本", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_Id"), new GUIContent("Id", "稳定标识，存档与谱面用；只能小写字母/数字/下划线"));
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_DisplayName"), new GUIContent("显示名"));
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_Description"), new GUIContent("描述"));
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_Rarity"), new GUIContent("稀有度"));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("演出", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_NoteType"), new GUIContent("键型", "Tap 单点 / Flick 滑动（暂按 Tap 结算）"));
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_NoteCount"), new GUIContent("音符数", "这张牌打几个音符，1..4，只影响谱面"));
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_Pattern"), new GUIContent("键位分布"));
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_Seed"), new GUIContent("随机种子", "0 = 用 Id 的稳定哈希"));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("表现", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_Icon"), new GUIContent("卡面图标"));
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_Accent"), new GUIContent("强调色"));

            if (m_Serialized.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(m_Selected);
                AssetDatabase.SaveAssetIfDirty(m_Selected);
            }
        }

        private void DrawEffect()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("功能（伤害等数值在效果资产里，不在卡牌上）", EditorStyles.boldLabel);

            m_Serialized.Update();
            EditorGUILayout.PropertyField(m_Serialized.FindProperty("m_Effect"), new GUIContent("效果", "CardEffect 资产：伤害/治疗/抽牌等"));
            if (m_Serialized.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(m_Selected);
                AssetDatabase.SaveAssetIfDirty(m_Selected);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("新建伤害效果", GUILayout.Width(120f)))
                {
                    var effect = CardAssetFactory.CreateDamageEffect(m_Selected.Id + "_damage", 5);
                    m_Selected.Effect = effect;
                    EditorUtility.SetDirty(m_Selected);
                    AssetDatabase.SaveAssetIfDirty(m_Selected);
                    RebuildSerialized();
                    GUIUtility.ExitGUI();
                }

                using (new EditorGUI.DisabledScope(m_Selected.Effect == null))
                {
                    if (GUILayout.Button("打开效果资产", GUILayout.Width(120f)))
                    {
                        Selection.activeObject = m_Selected.Effect;
                        EditorGUIUtility.PingObject(m_Selected.Effect);
                    }
                }

                GUILayout.FlexibleSpace();
                if (m_Selected.Effect != null)
                {
                    EditorGUILayout.LabelField(m_Selected.Effect.Summary, EditorStyles.miniLabel);
                }
            }
        }

        private void DrawValidation()
        {
            List<CardIssue> issues = CardValidator.Validate(m_Selected, m_Cards);
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("校验通过。", MessageType.Info);
                return;
            }

            foreach (CardIssue issue in issues)
            {
                EditorGUILayout.HelpBox(issue.Message, issue.Level == CardIssueLevel.Error ? MessageType.Error : MessageType.Warning);
            }
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("键位预览（D F J K）", EditorStyles.boldLabel);

            int[] lanes = m_Selected.GetLanes();
            EditorGUILayout.LabelField("分布：" + string.Join(" → ", lanes.Select(LaneName)), EditorStyles.miniLabel);

            Rect rect = GUILayoutUtility.GetRect(0f, 140f, GUILayout.ExpandWidth(true));
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            float labelWidth = 26f;
            float cellWidth = (rect.width - labelWidth) / PreviewColumns;
            float cellHeight = rect.height / LaneCount;

            for (int lane = 0; lane < LaneCount; lane++)
            {
                var laneRect = new Rect(rect.x, rect.y + lane * cellHeight, labelWidth, cellHeight);
                GUI.Label(laneRect, LaneName(lane), EditorStyles.miniLabel);

                for (int column = 0; column < PreviewColumns; column++)
                {
                    var cell = new Rect(rect.x + labelWidth + column * cellWidth, rect.y + lane * cellHeight, cellWidth - 2f, cellHeight - 2f);
                    EditorGUI.DrawRect(cell, new Color(0.16f, 0.17f, 0.19f, 1f));
                }
            }

            for (int i = 0; i < lanes.Length; i++)
            {
                int lane = Mathf.Clamp(lanes[i], 0, LaneCount - 1);
                int column = Mathf.Clamp(i, 0, PreviewColumns - 1);
                var note = new Rect(rect.x + labelWidth + column * cellWidth + 3f, rect.y + lane * cellHeight + 3f, cellWidth - 8f, cellHeight - 8f);
                Color color = m_Selected.Accent;
                color.a = 1f;
                EditorGUI.DrawRect(note, color);
            }
        }

        private bool Matches(CardDefinition card)
        {
            if (string.IsNullOrEmpty(m_Search))
            {
                return true;
            }

            string search = m_Search.Trim();
            return card.DisplayName.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0
                || card.Id.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string LaneName(int lane)
        {
            switch (lane)
            {
                case 0: return "D";
                case 1: return "F";
                case 2: return "J";
                default: return "K";
            }
        }
    }
}
