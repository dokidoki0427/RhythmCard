using System.Collections.Generic;
using CardRhythm.Cards;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorCharts
{
    /// <summary>生成一套测试 Buff（复用 CardDefinition 数据结构，运行时按 Buff 使用）。</summary>
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
            CreateBuff("buff_heavy", "重音", "一次沉重的打击。", new Color(0.86f, 0.32f, 0.30f), 40);
            CreateBuff("buff_chord", "和弦", "三个音同时落下。", new Color(0.92f, 0.55f, 0.20f), 60);
            CreateBuff("buff_light", "跳音", "轻快但频繁。", new Color(0.35f, 0.62f, 1f), 25);
            CreateBuff("buff_rest", "休止", "短暂的呼吸。", new Color(0.40f, 0.78f, 0.55f), 25, true);
            CreateBuff("buff_echo", "回声", "余音绕梁。", new Color(0.55f, 0.55f, 0.85f), 40, true);
            CreateBuff("buff_fermata", "延长", "一个长长的尾音。", new Color(0.80f, 0.70f, 0.95f), 55, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BUFF] generated");
        }

        private static void CreateBuff(string id, string name, string description, Color color, int power, bool heal = false)
        {
            string effectPath = EffectFolder + "/" + id + "_effect.asset";
            CardEffect effect = AssetDatabase.LoadAssetAtPath<CardEffect>(effectPath);

            if (heal)
            {
                var healEffect = effect as HealEffect;
                if (healEffect == null)
                {
                    healEffect = ScriptableObject.CreateInstance<HealEffect>();
                    AssetDatabase.CreateAsset(healEffect, effectPath);
                }

                healEffect.Heal = power;
                EditorUtility.SetDirty(healEffect);
                effect = healEffect;
            }
            else
            {
                var damageEffect = effect as DamageEffect;
                if (damageEffect == null)
                {
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
