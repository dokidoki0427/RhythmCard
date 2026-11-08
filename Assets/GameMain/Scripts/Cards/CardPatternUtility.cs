using System;
using UnityEngine;

namespace CardRhythm.Cards
{
    /// <summary>
    /// 卡牌 → 轨道分布。纯函数，不读资源、不依赖时间，方便单测和确定性复现。
    /// 轨道约定：0 = D，1 = F，2 = J，3 = K。
    /// </summary>
    public static class CardPatternUtility
    {
        private static readonly int[] MirrorOrder = { 1, 2, 0, 3 };

        public static int[] Lanes(int cost, CardPattern pattern, int seed)
        {
            cost = Mathf.Clamp(cost, 1, 4);
            var lanes = new int[cost];

            switch (pattern)
            {
                case CardPattern.Alternate:
                    for (int i = 0; i < cost; i++)
                    {
                        lanes[i] = i % 2 == 0 ? 0 : 3;
                    }

                    break;

                case CardPattern.Mirror:
                    for (int i = 0; i < cost; i++)
                    {
                        lanes[i] = MirrorOrder[i % MirrorOrder.Length];
                    }

                    break;

                case CardPattern.Random:
                    var random = new System.Random(seed);
                    for (int i = 0; i < cost; i++)
                    {
                        lanes[i] = random.Next(0, 4);
                    }

                    break;

                default:
                    for (int i = 0; i < cost; i++)
                    {
                        lanes[i] = i % 4;
                    }

                    break;
            }

            return lanes;
        }

        /// <summary>
        /// 稳定哈希（FNV-1a）。不用 string.GetHashCode：.NET 的字符串哈希可能按进程加盐，
        /// 同一个 Id 两次运行会得到不同的谱面。
        /// </summary>
        public static int StableHash(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            unchecked
            {
                const uint offset = 2166136261;
                const uint prime = 16777619;
                uint hash = offset;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= prime;
                }

                return (int)(hash & 0x7FFFFFFF);
            }
        }
    }
}
