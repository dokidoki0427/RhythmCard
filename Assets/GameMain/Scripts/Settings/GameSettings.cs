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

        /// <summary>向牌组追加一张（允许重复），达到上限返回 false。</summary>
        public static bool AppendToDeck(string id, int maxCards = 20)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            List<string> ids = LoadDeck();
            if (ids.Count >= maxCards)
            {
                return false;
            }

            ids.Add(id);
            PlayerPrefs.SetString(DeckKey, string.Join("|", ids));
            PlayerPrefs.Save();
            return true;
        }

        // ── 键位重绑 ──────────────────────────────────────────────

        private static readonly KeyCode[] DefaultKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };

        public static KeyCode GetLaneKey(int lane)
        {
            lane = Mathf.Clamp(lane, 0, DefaultKeys.Length - 1);
            return (KeyCode)PlayerPrefs.GetInt("rhythm.key." + lane, (int)DefaultKeys[lane]);
        }

        public static void SetLaneKey(int lane, KeyCode key)
        {
            lane = Mathf.Clamp(lane, 0, DefaultKeys.Length - 1);
            PlayerPrefs.SetInt("rhythm.key." + lane, (int)key);
            PlayerPrefs.Save();
        }

        public static bool IsLaneKeyTaken(KeyCode key, int exceptLane)
        {
            for (int lane = 0; lane < DefaultKeys.Length; lane++)
            {
                if (lane != exceptLane && GetLaneKey(lane) == key)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ResetLaneKeys()
        {
            for (int lane = 0; lane < DefaultKeys.Length; lane++)
            {
                PlayerPrefs.DeleteKey("rhythm.key." + lane);
            }

            PlayerPrefs.Save();
        }
    }
}
