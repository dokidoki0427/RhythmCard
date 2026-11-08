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
                CardAssetFactory.EnsureDamageEffect("wild_strike_damage", 18), CardNoteType.Tap, CardPattern.Random);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return CardAssetFactory.LoadAll().Count - before;
        }
    }
}
