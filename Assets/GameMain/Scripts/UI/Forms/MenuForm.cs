using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 开始界面。布局全部在预制体里（UIPrefabBuilderWindow 生成），
    /// 脚本只负责按钮回调与全屏拉伸。
    /// </summary>
    public sealed class MenuForm : UIFormBase
    {
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            Bind("StartButton", StartGame);
            Bind("DeckButton", OpenDeck);
            Bind("SettingsButton", OpenSettings);
            Bind("QuitButton", Quit);
        }

        private void Bind(string childName, System.Action action)
        {
            Transform child = transform.Find(childName);
            if (child == null)
            {
                return;
            }

            var button = child.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }

        private static void StartGame()
        {
            var procedure = CardRhythm.Procedure.GameProcedureBase.Current;
            if (procedure != null)
            {
                procedure.RequestAdvance();
            }
        }

        private static void OpenDeck()
        {
            UIRouter.Open(UIFormId.Deck, null, true, true);
        }

        private static void OpenSettings()
        {
            UIRouter.Open(UIFormId.Settings, null, true, true);
        }

        private static void Quit()
        {
            Application.Quit();
        }
    }
}
