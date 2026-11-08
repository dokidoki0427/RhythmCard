using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CardRhythm.Cards
{
    public enum CardIssueLevel
    {
        Warning = 0,
        Error = 1,
    }

    public sealed class CardIssue
    {
        public readonly CardIssueLevel Level;
        public readonly string Message;

        public CardIssue(CardIssueLevel level, string message)
        {
            Level = level;
            Message = message;
        }
    }

    /// <summary>
    /// 卡牌数据校验。纯逻辑，不依赖 UnityEditor，编辑器窗口与 CI 校验共用同一套规则。
    /// 只校验“卡牌身份 + 演出 + 效果引用”，效果内部的数值由效果自己的校验负责。
    /// </summary>
    public static class CardValidator
    {
        private static readonly Regex IdPattern = new Regex("^[a-z0-9_]+$");

        public static List<CardIssue> Validate(CardDefinition card, IEnumerable<CardDefinition> all)
        {
            var issues = new List<CardIssue>();
            if (card == null)
            {
                issues.Add(Error("卡牌为空"));
                return issues;
            }

            if (string.IsNullOrWhiteSpace(card.Id))
            {
                issues.Add(Error("Id 不能为空"));
            }
            else if (!IdPattern.IsMatch(card.Id))
            {
                issues.Add(Error("Id 只能包含小写字母、数字、下划线：" + card.Id));
            }

            if (string.IsNullOrWhiteSpace(card.DisplayName))
            {
                issues.Add(Error("显示名不能为空"));
            }

            if (card.NoteCount < 1 || card.NoteCount > 4)
            {
                issues.Add(Error("音符数必须在 1..4"));
            }

            if (card.Effect == null)
            {
                issues.Add(Warning("没有绑定功能效果，战斗时这张牌不会产生作用"));
            }

            if (card.Icon == null)
            {
                issues.Add(Warning("还没有卡面图标（不影响数值，但列表里会显示默认色块）"));
            }

            if (card.NoteType == CardNoteType.Flick)
            {
                issues.Add(Warning("Flick 目前按 Tap 结算，等滑动判定实现后再启用"));
            }

            if (all != null)
            {
                int sameId = 0;
                foreach (CardDefinition other in all)
                {
                    if (other != null && other != card && other.Id == card.Id)
                    {
                        sameId++;
                    }
                }

                if (sameId > 0)
                {
                    issues.Add(Error("Id 重复：" + card.Id + "（另有 " + sameId + " 张卡用了同一个 Id）"));
                }
            }

            return issues;
        }

        public static bool HasError(List<CardIssue> issues)
        {
            foreach (CardIssue issue in issues)
            {
                if (issue.Level == CardIssueLevel.Error)
                {
                    return true;
                }
            }

            return false;
        }

        private static CardIssue Error(string message)
        {
            return new CardIssue(CardIssueLevel.Error, message);
        }

        private static CardIssue Warning(string message)
        {
            return new CardIssue(CardIssueLevel.Warning, message);
        }
    }
}
