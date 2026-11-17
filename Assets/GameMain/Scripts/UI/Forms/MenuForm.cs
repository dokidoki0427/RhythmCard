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
            FixNoteHeads();
        }

        /// <summary>预制体里的音符头如果没有 Sprite 会显示成方块，运行时统一换成圆形。</summary>
        private void FixNoteHeads()
        {
            foreach (Image image in GetComponentsInChildren<Image>(true))
            {
                if (image.gameObject.name == "NoteHead")
                {
                    image.sprite = SheetTheme.Circle;
                    image.type = Image.Type.Simple;
                    image.preserveAspect = true;
                }
            }
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
