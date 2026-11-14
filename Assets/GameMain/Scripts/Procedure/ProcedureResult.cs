using CardRhythm.Battle;
using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace CardRhythm.Procedure
{
    /// <summary>
    /// 结算：胜利后先三选一奖励，再推进一层；失败则重置进度回菜单。
    /// </summary>
    public sealed class ProcedureResult : GameProcedureBase
    {
        private bool m_RewardShown;
        private bool m_WaitingReward;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_RewardShown = false;
            m_WaitingReward = false;
            UIRouter.Open(UIFormId.Result, BattleResult.Last);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            UIRouter.Close(UIFormId.Result);
            UIRouter.Close(UIFormId.Reward);
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_WaitingReward && !UIRouter.HasModal)
            {
                m_WaitingReward = false;
                RunState.Advance();
                ChangeTo<ProcedureMap>();
            }
        }

        public override void RequestAdvance()
        {
            BattleResult result = BattleResult.Last;
            if (result != null && result.Victory && !m_RewardShown)
            {
                m_RewardShown = true;
                m_WaitingReward = true;
                UIRouter.Open(UIFormId.Reward, null, true, true);
                return;
            }

            if (result != null && result.Victory)
            {
                RunState.Advance();
            }
            else
            {
                RunState.ResetRun();
            }

            ChangeTo<ProcedureMap>();
        }
    }
}
