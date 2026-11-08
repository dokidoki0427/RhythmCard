using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CardRhythm.Cards;
using UnityEditor;
using UnityEngine;

namespace CardRhythm.EditorCards
{
    /// <summary>
    /// 卡牌与效果资产的增删改查。窗口和批处理都用这里，避免两套逻辑。
    /// </summary>
    public static class CardAssetFactory
    {
        public const string Folder = "Assets/GameMain/Data/Cards";
        public const string EffectFolder = "Assets/GameMain/Data/Effects";

        public static List<CardDefinition> LoadAll()
        {
            EnsureFolder(Folder);
            var cards = new List<CardDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:CardDefinition", new[] { Folder }))
            {
                var card = AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (card != null)
                {
                    cards.Add(card);
                }
            }

            return cards.OrderBy(c => c.Id).ToList();
        }

        public static CardDefinition Create(string displayName)
        {
            EnsureFolder(Folder);

            var card = ScriptableObject.CreateInstance<CardDefinition>();
            card.DisplayName = string.IsNullOrWhiteSpace(displayName) ? "新卡牌" : displayName.Trim();
            card.Id = MakeUniqueId(Slug(card.DisplayName));

            string path = AssetDatabase.GenerateUniqueAssetPath(Folder + "/" + card.Id + ".asset");
            AssetDatabase.CreateAsset(card, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = card;
            return card;
        }

        /// <summary>按固定 Id 创建（示例卡用）；已存在则直接返回，不重复创建。</summary>
        public static CardDefinition Ensure(string id, string displayName, int noteCount, CardEffect effect,
            CardNoteType noteType = CardNoteType.Tap, CardPattern pattern = CardPattern.Sequence)
        {
            var existing = LoadAll().FirstOrDefault(c => c.Id == id);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder(Folder);
            var card = ScriptableObject.CreateInstance<CardDefinition>();
            card.Id = id;
            card.DisplayName = displayName;
            card.NoteCount = noteCount;
            card.Effect = effect;
            card.NoteType = noteType;
            card.Pattern = pattern;

            string path = AssetDatabase.GenerateUniqueAssetPath(Folder + "/" + id + ".asset");
            AssetDatabase.CreateAsset(card, path);
            AssetDatabase.SaveAssets();
            return card;
        }

        public static CardDefinition Duplicate(CardDefinition source)
        {
            if (source == null)
            {
                return null;
            }

            EnsureFolder(Folder);
            var copy = Object.Instantiate(source);
            copy.name = source.name + "_copy";
            copy.DisplayName = source.DisplayName + " 副本";
            copy.Id = MakeUniqueId(source.Id + "_copy");

            string path = AssetDatabase.GenerateUniqueAssetPath(Folder + "/" + copy.Id + ".asset");
            AssetDatabase.CreateAsset(copy, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = copy;
            return copy;
        }

        public static void Delete(CardDefinition card)
        {
            if (card == null)
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(card);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
        }

        /// <summary>把资产文件名改成 Id，方便在 Project 窗口里一眼认出来。</summary>
        public static string RenameAssetToId(CardDefinition card)
        {
            if (card == null)
            {
                return null;
            }

            string path = AssetDatabase.GetAssetPath(card);
            string error = AssetDatabase.RenameAsset(path, card.Id);
            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            AssetDatabase.SaveAssets();
            return null;
        }

        public static DamageEffect CreateDamageEffect(string preferredId, int damage)
        {
            EnsureFolder(EffectFolder);
            var effect = ScriptableObject.CreateInstance<DamageEffect>();
            effect.Damage = damage;

            string baseId = Slug(preferredId);
            if (string.IsNullOrEmpty(baseId))
            {
                baseId = "damage_effect";
            }

            string path = AssetDatabase.GenerateUniqueAssetPath(EffectFolder + "/" + baseId + ".asset");
            AssetDatabase.CreateAsset(effect, path);
            AssetDatabase.SaveAssets();
            return effect;
        }

        /// <summary>幂等：同 Id 的效果已存在则复用。</summary>
        public static DamageEffect EnsureDamageEffect(string id, int damage)
        {
            EnsureFolder(EffectFolder);
            foreach (string guid in AssetDatabase.FindAssets("t:DamageEffect", new[] { EffectFolder }))
            {
                var existing = AssetDatabase.LoadAssetAtPath<DamageEffect>(AssetDatabase.GUIDToAssetPath(guid));
                if (existing != null && existing.name == id)
                {
                    return existing;
                }
            }

            return CreateDamageEffect(id, damage);
        }

        public static string MakeUniqueId(string preferred)
        {
            var used = new HashSet<string>();
            foreach (CardDefinition card in LoadAll())
            {
                if (card != null && !string.IsNullOrEmpty(card.Id))
                {
                    used.Add(card.Id);
                }
            }

            string baseId = Slug(preferred);
            if (string.IsNullOrEmpty(baseId))
            {
                baseId = "card";
            }

            if (!used.Contains(baseId))
            {
                return baseId;
            }

            for (int i = 2; i < 1000; i++)
            {
                string candidate = baseId + "_" + i;
                if (!used.Contains(candidate))
                {
                    return candidate;
                }
            }

            return baseId + "_" + System.Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        /// <summary>显示名 → 合法 Id：小写、去中文、非法字符变下划线。</summary>
        public static string Slug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (char c in value.Trim().ToLowerInvariant())
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    builder.Append(c);
                }
                else if (c == ' ' || c == '-' || c == '_')
                {
                    builder.Append('_');
                }
            }

            string result = builder.ToString().Trim('_');
            while (result.Contains("__"))
            {
                result = result.Replace("__", "_");
            }

            return result;
        }

        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string leaf = Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
