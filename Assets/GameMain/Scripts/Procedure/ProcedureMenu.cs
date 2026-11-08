using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace CardRhythm.Procedure
{
    /// <summary>主菜单：显示 MenuForm，Space / 按钮进入地图。</summary>
    public sealed class ProcedureMenu : GameProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            UIRouter.Open(UIFormId.Menu);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            UIRouter.Close(UIFormId.Menu);
            base.OnLeave(procedureOwner, isShutdown);
        }

        public override void RequestAdvance()
        {
            ChangeTo<ProcedureMap>();
        }
    }
}
