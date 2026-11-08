using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace CardRhythm.Procedure
{
    /// <summary>结算：显示 ResultForm，Space 回地图（骨架阶段用来验证闭环）。</summary>
    public sealed class ProcedureResult : GameProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            UIRouter.Open(UIFormId.Result);
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
