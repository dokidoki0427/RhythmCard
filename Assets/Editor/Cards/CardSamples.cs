using CardRhythm.Cards;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorCards
{
    /// <summary>
    /// 示例卡牌：只用于验证编辑器与后续战斗，正式内容请用卡牌编辑器自己建。
    /// 菜单：Tools/卡牌音游/生成示例卡牌（幂等，已存在则跳过）
    /// </summary>
    public static class CardSamples
    {
        [MenuItem("Tools/卡牌音游/生成示例卡牌", false, 101)]
        public static void CreateIfMissingMenu()
        {
            int created = CreateIfMissing();
            EditorUtility.DisplayDialog("示例卡牌", "新增 " + created + " 张（已存在的不重复创建）。", "好");
        }

        /// <summary>批处理入口。</summary>
        public static void CreateIfMissingBatch()
        {
            int created = CreateIfMissing();
            Debug.Log("[CARDS] samples created=" + created);
            EditorApplication.Exit(0);
        }

        public static int CreateIfMissing()
        {
            int before = CardAssetFactory.LoadAll().Count;

            CardAssetFactory.Ensure("slash", "斩击", 1,
                CardAssetFactory.EnsureDamageEffect("slash_damage", 3), CardNoteType.Tap, CardPattern.Sequence);
            CardAssetFactory.Ensure("double_hit", "双击", 2,
                CardAssetFactory.EnsureDamageEffect("double_hit_damage", 7), CardNoteType.Tap, CardPattern.Alternate);
            CardAssetFactory.Ensure("heavy_slash", "重斩", 3,
                CardAssetFactory.EnsureDamageEffect("heavy_slash_damage", 12), CardNoteType.Tap, CardPattern.Mirror);
            CardAssetFactory.Ensure("wild_strike", "乱击", 4,
                CardAssetFactory.EnsureDamageEffect("wild_strike_damage", 18), CardNoteType.Tap, CardPattern.Random,
                new Color(0.95f, 0.45f, 0.85f));

            CardAssetFactory.Ensure("pierce", "穿刺", 2,
                CardAssetFactory.EnsureDamageEffect("pierce_damage", 9), CardNoteType.Tap, CardPattern.Sequence,
                new Color(0.6f, 0.9f, 0.45f));
            CardAssetFactory.Ensure("sweep", "横扫", 3,
                CardAssetFactory.EnsureDamageEffect("sweep_damage", 14), CardNoteType.Tap, CardPattern.Alternate,
                new Color(1f, 0.65f, 0.3f));
            CardAssetFactory.Ensure("burst", "爆发", 4,
                CardAssetFactory.EnsureDamageEffect("burst_damage", 22), CardNoteType.Tap, CardPattern.Random,
                new Color(1f, 0.4f, 0.5f));
            CardAssetFactory.Ensure("jab", "刺击", 1,
                CardAssetFactory.EnsureDamageEffect("jab_damage", 4), CardNoteType.Tap, CardPattern.Sequence,
                new Color(0.5f, 0.8f, 1f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return CardAssetFactory.LoadAll().Count - before;
        }
    }
}
