using CardRhythm.UI;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;

namespace CardRhythm.Procedure
{
    /// <summary>
    /// 启动：清空界面、等一帧让框架/资源就绪，然后进主菜单。
    /// 以后这里会放：读取设置、加载本地化、检查资源版本。
    /// </summary>
    public sealed class ProcedureLaunch : GameProcedureBase
    {
        private float m_Elapsed;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_Elapsed = 0f;
            UIRouter.CloseAll();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            m_Elapsed += realElapseSeconds;
            if (m_Elapsed >= 0.2f)
            {
                ChangeTo<ProcedureMenu>();
            }
        }
    }
}
