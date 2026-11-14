using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;

namespace CardRhythm.Procedure
{
    /// <summary>地图 / 选路：Space 进战斗；O 设置，B 牌组。</summary>
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
            ChangeTo<ProcedureBattle>();
        }
    }
}
