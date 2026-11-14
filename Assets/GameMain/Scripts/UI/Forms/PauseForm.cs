using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>暂停菜单：继续 / 设置 / 退出到菜单。暂停由 BattleRunner 控制 dspTime。</summary>
    public sealed class PauseForm : UIFormBase
    {
        private bool m_Built;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (!m_Built)
            {
                BuildUi();
                m_Built = true;
            }
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        private void BuildUi()
        {
            var panel = UIWidgets.Panel(transform, "PausePanel", new Vector2(720f, 560f), Vector2.zero,
                new Color(0.08f, 0.09f, 0.12f, 0.98f));

            UIWidgets.Label(panel, "PAUSED", 46, TextAnchor.MiddleCenter, Color.white,
                new Vector2(600f, 70f), new Vector2(0f, 190f));

            UIWidgets.Button(panel, "继续", new Vector2(360f, 66f), new Vector2(0f, 70f), Close,
                new Color(0.22f, 0.32f, 0.28f));
            UIWidgets.Button(panel, "设置", new Vector2(360f, 66f), new Vector2(0f, -20f), OpenSettings,
                new Color(0.2f, 0.24f, 0.32f));
            UIWidgets.Button(panel, "退出到菜单", new Vector2(360f, 66f), new Vector2(0f, -110f), QuitToMenu,
                new Color(0.32f, 0.2f, 0.2f));
        }

        private void OpenSettings()
        {
            UIRouter.Open(UIFormId.Settings, null, true, true);
        }

        private void QuitToMenu()
        {
            if (Rhythm.BattleRunner.Current != null)
            {
                Rhythm.BattleRunner.Current.RequestAbort();
            }
            else
            {
                Close();
            }
        }
    }
}
