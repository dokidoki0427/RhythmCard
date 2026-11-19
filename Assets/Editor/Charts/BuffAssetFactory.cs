using CardRhythm.Cards;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorCharts
{
    /// <summary>生成测试 Buff：3 个攻击（伤害）+ 3 个防御（护甲）。</summary>
    public static class BuffAssetFactory
    {
        private const string BuffFolder = "Assets/GameMain/Resources/Buffs";
        private const string EffectFolder = "Assets/GameMain/Data/Buffs";

        [MenuItem("Tools/卡牌音游/谱面/生成测试 Buff", false, 71)]
        public static void GenerateMenu()
        {
            GenerateAll();
            EditorUtility.DisplayDialog("Buff", "已生成 6 个测试 Buff。", "好");
        }

        public static void GenerateBatch()
        {
            GenerateAll();
            EditorApplication.Exit(0);
        }

        public static void GenerateAll()
        {
            EnsureFolder();

            CreateBuff("buff_heavy", "重音", "沉重的打击。", new Color(0.90f, 0.35f, 0.30f), 40, BuffType.Attack, 40);
            CreateBuff("buff_chord", "和弦", "三个音同时落下。", new Color(0.95f, 0.55f, 0.20f), 60, BuffType.Attack, 60);
            CreateBuff("buff_light", "跳音", "轻快但频繁。", new Color(0.90f, 0.30f, 0.45f), 25, BuffType.Attack, 25);

            CreateBuff("buff_guard", "护音", "一小段护甲。", new Color(0.35f, 0.65f, 0.95f), 30, BuffType.Defense, 30);
            CreateBuff("buff_aegis", "守调", "稳固的护甲。", new Color(0.30f, 0.80f, 0.85f), 45, BuffType.Defense, 45);
            CreateBuff("buff_bastion", "壁垒", "厚实的护甲。", new Color(0.55f, 0.55f, 0.95f), 60, BuffType.Defense, 60);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BUFF] generated");
        }

        private static void CreateBuff(string id, string name, string description, Color color, int power, BuffType type, int armor)
        {
            string effectPath = EffectFolder + "/" + id + "_effect.asset";
            CardEffect effect = AssetDatabase.LoadAssetAtPath<CardEffect>(effectPath);

            if (type == BuffType.Defense)
            {
                var armorEffect = effect as ArmorEffect;
                if (armorEffect == null)
                {
                    if (effect != null)
                    {
                        AssetDatabase.DeleteAsset(effectPath);
                    }

                    armorEffect = ScriptableObject.CreateInstance<ArmorEffect>();
                    AssetDatabase.CreateAsset(armorEffect, effectPath);
                }

                armorEffect.Armor = armor;
                EditorUtility.SetDirty(armorEffect);
                effect = armorEffect;
            }
            else
            {
                var damageEffect = effect as DamageEffect;
                if (damageEffect == null)
                {
                    if (effect != null)
                    {
                        AssetDatabase.DeleteAsset(effectPath);
                    }

                    damageEffect = ScriptableObject.CreateInstance<DamageEffect>();
                    AssetDatabase.CreateAsset(damageEffect, effectPath);
                }

                damageEffect.Damage = power;
                EditorUtility.SetDirty(damageEffect);
                effect = damageEffect;
            }

            string path = BuffFolder + "/" + id + ".asset";
            var buff = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
            if (buff == null)
            {
                buff = ScriptableObject.CreateInstance<CardDefinition>();
                AssetDatabase.CreateAsset(buff, path);
            }

            buff.Id = id;
            buff.DisplayName = name;
            buff.Description = description;
            buff.NoteCount = 4;
            buff.Accent = color;
            buff.BuffType = type;
            buff.Effect = effect;
            EditorUtility.SetDirty(buff);
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameMain/Data/Buffs"))
            {
                AssetDatabase.CreateFolder("Assets/GameMain/Data", "Buffs");
            }

            if (!AssetDatabase.IsValidFolder("Assets/GameMain/Resources/Buffs"))
            {
                AssetDatabase.CreateFolder("Assets/GameMain/Resources", "Buffs");
            }
        }
    }
}
