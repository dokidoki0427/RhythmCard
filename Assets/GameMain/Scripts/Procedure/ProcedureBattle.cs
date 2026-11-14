using CardRhythm.Rhythm;
using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;

namespace CardRhythm.Procedure
{
    /// <summary>战斗：生成谱面、跑判定，打完（或按 Space 跳过）进结算。</summary>
    public sealed class ProcedureBattle : GameProcedureBase
    {
        private const float Bpm = 120f;
        private const int Bars = 32;

        private BattleRunner m_Runner;
        private GameObject m_BattleRoot;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            UIRouter.Open(UIFormId.BattleHud);

            m_BattleRoot = new GameObject("Battle");
            m_Runner = m_BattleRoot.AddComponent<BattleRunner>();
            m_Runner.Begin(Bpm, Bars);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            if (m_BattleRoot != null)
            {
                Object.Destroy(m_BattleRoot);
                m_BattleRoot = null;
            }

            m_Runner = null;
            UIRouter.Close(UIFormId.BattleHud);
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_Runner != null && m_Runner.IsFinished)
            {
                ChangeTo<ProcedureResult>();
            }
        }

        /// <summary>战斗里 Space 用于强拍，不参与流程推进。</summary>
        protected override bool SpaceAdvanceEnabled
        {
            get { return false; }
        }

        /// <summary>Space 仍然可以跳过战斗（调试用）。</summary>
        public override void RequestAdvance()
        {
            ChangeTo<ProcedureResult>();
        }
    }
}
