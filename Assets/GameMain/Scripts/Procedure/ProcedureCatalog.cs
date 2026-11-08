namespace CardRhythm.Procedure
{
    /// <summary>
    /// 流程清单。Boot 场景里的 ProcedureComponent 必须按这里的类型名初始化，
    /// 顺序也要保持一致（GF 按数组顺序注册）。
    /// </summary>
    public static class ProcedureCatalog
    {
        public static readonly string[] AvailableTypeNames =
        {
            typeof(ProcedureLaunch).FullName,
            typeof(ProcedureMenu).FullName,
            typeof(ProcedureMap).FullName,
            typeof(ProcedureBattle).FullName,
            typeof(ProcedureResult).FullName,
        };

        public static readonly string EntranceTypeName = typeof(ProcedureLaunch).FullName;
    }
}
