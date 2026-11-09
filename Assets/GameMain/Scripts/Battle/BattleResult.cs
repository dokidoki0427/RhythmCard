namespace CardRhythm.Battle
{
    /// <summary>一场战斗的结算数据，由 ProcedureBattle 传给 ProcedureResult。</summary>
    public sealed class BattleResult
    {
        public bool Victory;
        public int Score;
        public int MaxCombo;
        public int Perfect;
        public int Good;
        public int Bad;
        public int Miss;
        public float Accuracy;

        /// <summary>上一场战斗的结果（流程之间传参用；同时只有一场战斗）。</summary>
        public static BattleResult Last { get; set; }

        public static BattleResult From(BattleState state, bool victory)
        {
            return new BattleResult
            {
                Victory = victory,
                Score = state.Score,
                MaxCombo = state.MaxCombo,
                Perfect = state.Perfect,
                Good = state.Good,
                Bad = state.Bad,
                Miss = state.Miss,
                Accuracy = state.Accuracy,
            };
        }
    }
}
