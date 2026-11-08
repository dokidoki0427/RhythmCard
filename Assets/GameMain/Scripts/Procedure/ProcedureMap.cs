using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace CardRhythm.Procedure
{
    /// <summary>地图 / 选路：目前只是占位界面，Space 直接进战斗。</summary>
    public sealed class ProcedureMap : GameProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            UIRouter.Open(UIFormId.Map);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            UIRouter.Close(UIFormId.Map);
            base.OnLeave(procedureOwner, isShutdown);
        }

        public override void RequestAdvance()
        {
            ChangeTo<ProcedureBattle>();
        }
    }
}
