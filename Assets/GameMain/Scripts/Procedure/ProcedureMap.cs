using CardRhythm.Battle;
using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;

namespace CardRhythm.Procedure
{
    /// <summary>地图：Space 进入当前节点；事件节点直接推进；O 设置，B 牌组。</summary>
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
            if (RunState.CurrentNode == MapNodeType.Event)
            {
                // 事件层：原型期直接跳过（后续接事件内容），就地刷新地图。
                RunState.Advance();
                RefreshMapForm();
                return;
            }

            ChangeTo<ProcedureBattle>();
        }

        private static void RefreshMapForm()
        {
            var uiComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();
            if (uiComponent == null)
            {
                return;
            }

            UIFormDef def = UIFormCatalog.Get(UIFormId.Map);
            var form = uiComponent.GetUIForm(def.AssetPath);
            if (form != null)
            {
                var mapForm = form.GetComponent<MapForm>();
                if (mapForm != null)
                {
                    mapForm.Refresh();
                }
            }
        }
    }
}
