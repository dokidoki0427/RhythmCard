using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;

namespace CardRhythm.Procedure
{
    /// <summary>
    /// 流程基类。GF 的 ProcedureBase 基于 FsmState，这里补三件事：
    /// 1. 记录当前流程实例（给调试覆盖层/外部查询用）；
    /// 2. 统一用 Space 推进（骨架阶段没有真实按钮，先用键盘验证流程能走通）；
    /// 3. 提供 ChangeTo&lt;T&gt; 简化状态切换。
    /// </summary>
    public abstract class GameProcedureBase : ProcedureBase
    {
        public static GameProcedureBase Current { get; private set; }

        protected IFsm<IProcedureManager> Owner { get; private set; }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Owner = procedureOwner;
            Current = this;
            Debug.Log("[FLOW] enter " + GetType().Name);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            if (Current == this)
            {
                Current = null;
            }

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                RequestAdvance();
            }
        }

        /// <summary>Space 推进。各流程自己决定下一步。</summary>
        public virtual void RequestAdvance()
        {
        }

        protected void ChangeTo<T>() where T : ProcedureBase
        {
            ChangeState<T>(Owner);
        }
    }
}
