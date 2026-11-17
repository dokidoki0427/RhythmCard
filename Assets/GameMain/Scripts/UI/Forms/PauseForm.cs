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
            var panel = transform.Find("Panel") as RectTransform;
            if (panel == null)
            {
                panel = UIWidgets.Panel(transform, "PausePanel", new Vector2(720f, 560f), Vector2.zero, SheetTheme.Parchment);
            }

            var panelImage = panel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = SheetTheme.Parchment;
            }

            UIWidgets.Label(panel, "暂 停", 46, TextAnchor.MiddleCenter, SheetTheme.Ink,
                new Vector2(600f, 70f), new Vector2(0f, 190f));

            UIWidgets.Label(panel, "第 " + CardRhythm.Battle.RunState.Layer + " 层 / " + CardRhythm.Battle.RunState.MaxLayer
                    + "   ·   牌组 " + CardRhythm.Settings.GameSettings.LoadDeck().Count + " 张",
                24, TextAnchor.MiddleCenter, SheetTheme.InkSoft,
                new Vector2(600f, 40f), new Vector2(0f, 138f));

            UIWidgets.Button(panel, "继续", new Vector2(360f, 66f), new Vector2(0f, 70f), Close,
                SheetTheme.Ink, SheetTheme.Parchment, 28);
            UIWidgets.Button(panel, "设置", new Vector2(360f, 66f), new Vector2(0f, -20f), OpenSettings,
                SheetTheme.ParchmentDark, SheetTheme.Ink, 26);
            UIWidgets.Button(panel, "退出到菜单", new Vector2(360f, 66f), new Vector2(0f, -110f), QuitToMenu,
                SheetTheme.Crimson, SheetTheme.Parchment, 26);
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
