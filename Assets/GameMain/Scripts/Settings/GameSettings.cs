using System.Collections.Generic;
using UnityEngine;

namespace CardRhythm.Settings
{
    /// <summary>
    /// 本地设置：判定 offset 与牌组选择，存 PlayerPrefs。
    /// offset 单位毫秒，正数表示“玩家习惯性按晚”，判定时从输入时间里减掉。
    /// </summary>
    public static class GameSettings
    {
        public const string OffsetKey = "rhythm.offsetMs";
        public const string DeckKey = "rhythm.deckIds";

        public static int OffsetMs
        {
            get { return PlayerPrefs.GetInt(OffsetKey, 0); }
            set
            {
                PlayerPrefs.SetInt(OffsetKey, Mathf.Clamp(value, -300, 300));
                PlayerPrefs.Save();
            }
        }

        public static float OffsetSeconds
        {
            get { return OffsetMs / 1000f; }
        }

        public static List<string> LoadDeck()
        {
            var ids = new List<string>();
            string raw = PlayerPrefs.GetString(DeckKey, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return ids;
            }

            foreach (string part in raw.Split('|'))
            {
                if (!string.IsNullOrEmpty(part))
                {
                    ids.Add(part);
                }
            }

            return ids;
        }

        public static void SaveDeck(IEnumerable<string> ids)
        {
            var list = new List<string>();
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id) && !list.Contains(id))
                {
                    list.Add(id);
                }
            }

            PlayerPrefs.SetString(DeckKey, string.Join("|", list));
            PlayerPrefs.Save();
        }

        public static void ClearDeck()
        {
            PlayerPrefs.DeleteKey(DeckKey);
            PlayerPrefs.Save();
        }
    }
}
