using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;

namespace CardRhythm.Procedure
{
    /// <summary>主菜单：显示 MenuForm，Space 进入地图；O 打开设置，B 打开牌组。</summary>
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

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!UIRouter.HasModal && Input.GetKeyDown(KeyCode.O))
            {
                UIRouter.Open(UIFormId.Settings, null, true, true);
            }

            if (!UIRouter.HasModal && Input.GetKeyDown(KeyCode.B))
            {
                UIRouter.Open(UIFormId.Deck, null, true, true);
            }
        }

        public override void RequestAdvance()
        {
            ChangeTo<ProcedureMap>();
        }
    }
}
