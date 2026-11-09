using CardRhythm.Battle;
using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace CardRhythm.Procedure
{
    /// <summary>结算：把上一场战斗的结果交给 ResultForm 显示，Space 回地图。</summary>
    public sealed class ProcedureResult : GameProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            UIRouter.Open(UIFormId.Result, BattleResult.Last);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            UIRouter.Close(UIFormId.Result);
            base.OnLeave(procedureOwner, isShutdown);
        }

        public override void RequestAdvance()
        {
            ChangeTo<ProcedureMap>();
        }
    }
}
