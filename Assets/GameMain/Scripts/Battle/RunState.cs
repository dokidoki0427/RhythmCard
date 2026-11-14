using UnityEngine;

namespace CardRhythm.Battle
{
    public enum MapNodeType
    {
        Battle = 0,
        Event = 1,
        Elite = 2,
        Boss = 3,
    }

    /// <summary>
    /// 一局跑图的进度（8 层线性），存 PlayerPrefs。
    /// 节点与敌人固定映射；事件节点不打架，直接推进。
    /// </summary>
    public static class RunState
    {
        public const int MaxLayer = 8;
        private const string LayerKey = "run.layer";

        /// <summary>8 层节点：战斗 5、事件 1、精英 1、Boss 1。</summary>
        public static readonly MapNodeType[] Nodes =
        {
            MapNodeType.Battle,
            MapNodeType.Battle,
            MapNodeType.Event,
            MapNodeType.Battle,
            MapNodeType.Elite,
            MapNodeType.Battle,
            MapNodeType.Event,
            MapNodeType.Boss,
        };

        /// <summary>与节点一一对应的敌人 Id（事件层为空）。</summary>
        public static readonly string[] EnemyIds =
        {
            "attacker",
            "attacker",
            string.Empty,
            "disruptor",
            "shielded",
            "attacker",
            string.Empty,
            "boss",
        };

        public static int Layer
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(LayerKey, 1), 1, MaxLayer); }
            set
            {
                PlayerPrefs.SetInt(LayerKey, Mathf.Clamp(value, 1, MaxLayer));
                PlayerPrefs.Save();
            }
        }

        public static MapNodeType CurrentNode
        {
            get { return Nodes[Mathf.Clamp(Layer - 1, 0, Nodes.Length - 1)]; }
        }

        public static string CurrentEnemyId
        {
            get { return EnemyIds[Mathf.Clamp(Layer - 1, 0, EnemyIds.Length - 1)]; }
        }

        public static bool IsLastLayer
        {
            get { return Layer >= MaxLayer; }
        }

        /// <summary>推进一层；Boss 之后回到第 1 层（新一局）。</summary>
        public static void Advance()
        {
            Layer = IsLastLayer ? 1 : Layer + 1;
        }

        public static void ResetRun()
        {
            Layer = 1;
        }
    }
}
