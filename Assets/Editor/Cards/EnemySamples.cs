using CardRhythm.Cards;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorCards
{
    /// <summary>
    /// 敌人模板：4 个占位敌人，覆盖三种行为。幂等。
    /// 菜单：Tools/卡牌音游/生成敌人模板
    /// </summary>
    public static class EnemySamples
    {
        private const string Folder = "Assets/GameMain/Resources/Enemies";

        [MenuItem("Tools/卡牌音游/生成敌人模板", false, 102)]
        public static void CreateMenu()
        {
            int created = CreateAll();
            EditorUtility.DisplayDialog("敌人模板", "新增 " + created + " 个（已存在的不重复创建）。", "好");
        }

        public static void CreateBatch()
        {
            int created = CreateAll();
            Debug.Log("[ENEMIES] created=" + created);
            EditorApplication.Exit(0);
        }

        public static int CreateAll()
        {
            EnsureFolder();
            int before = Count();

            Ensure("attacker", "训练木桩", 300, 10, 2, EnemyBehavior.Attacker, 0, 8, 0);
            Ensure("disruptor", "干扰者", 260, 8, 1, EnemyBehavior.Disruptor, 0, 8, 6);
            Ensure("shielded", "铁壁", 240, 12, 2, EnemyBehavior.Shielded, 80, 8, 0);
            Ensure("boss", "章节首领", 520, 15, 2, EnemyBehavior.Attacker, 0, 8, 0);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return Count() - before;
        }

        private static void Ensure(string id, string displayName, int hp, int attack, int interval,
            EnemyBehavior behavior, int shield, int periodicBars, int periodicDamage)
        {
            string path = Folder + "/" + id + ".asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path) != null)
            {
                return;
            }

            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.Id = id;
            enemy.DisplayName = displayName;
            enemy.MaxHp = hp;
            enemy.Attack = attack;
            enemy.NonPerfectPerAttack = interval;
            enemy.Behavior = behavior;
            enemy.ShieldHp = shield;
            enemy.PeriodicBars = periodicBars;
            enemy.PeriodicDamage = periodicDamage;
            AssetDatabase.CreateAsset(enemy, path);
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/GameMain/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets/GameMain", "Resources");
                }

                AssetDatabase.CreateFolder("Assets/GameMain/Resources", "Enemies");
            }
        }

        private static int Count()
        {
            return AssetDatabase.FindAssets("t:EnemyDefinition", new[] { Folder }).Length;
        }
    }
}
