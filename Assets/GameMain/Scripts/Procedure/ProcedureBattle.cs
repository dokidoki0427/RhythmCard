using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace CardRhythm.Procedure
{
    /// <summary>战斗：目前只开 HUD，Space 直接判胜进入结算。判定内核接进来后由胜负驱动。</summary>
    public sealed class ProcedureBattle : GameProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            UIRouter.Open(UIFormId.BattleHud);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            UIRouter.Close(UIFormId.BattleHud);
            base.OnLeave(procedureOwner, isShutdown);
        }

        public override void RequestAdvance()
        {
            ChangeTo<ProcedureResult>();
        }
    }
}
