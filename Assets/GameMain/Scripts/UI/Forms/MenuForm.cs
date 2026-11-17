using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 开始界面：羊皮纸底 + 五线谱标题。
    /// 按钮：开始游戏 / 牌组 / 设置 / 退出。
    /// </summary>
    public sealed class MenuForm : UIFormBase
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

            var background = GetComponent<Image>();
            if (background != null)
            {
                background.color = SheetTheme.Parchment;
            }

            HideTemplateChild("Title");
            HideTemplateChild("Hint");
        }

        private void BuildUi()
        {
            var root = SheetTheme.CreateRect(transform, "MenuRoot");
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.sizeDelta = Vector2.zero;
            root.anchoredPosition = Vector2.zero;

            // 纸张暗角
            SheetTheme.Fill(root, "TopShade", new Vector2(1920f, 120f), new Vector2(0f, 480f), new Color(0f, 0f, 0f, 0.05f));
            SheetTheme.Fill(root, "BottomShade", new Vector2(1920f, 120f), new Vector2(0f, -480f), new Color(0f, 0f, 0f, 0.05f));

            // 标题
            UIWidgets.Label(root, "卡牌音游", 110, TextAnchor.MiddleCenter, SheetTheme.Ink,
                new Vector2(1200f, 180f), new Vector2(0f, 430f));
            UIWidgets.Label(root, "DECK  IS  THE  SCORE", 30, TextAnchor.MiddleCenter, SheetTheme.InkSoft,
                new Vector2(900f, 50f), new Vector2(0f, 330f));

            // 五线谱 + 音符
            SheetTheme.Staff(root, new Vector2(0f, 235f), 1180f, 34f, new Color(0.15f, 0.13f, 0.11f, 0.35f));
            float[] noteX = { -420f, -210f, -40f, 160f, 360f };
            float[] noteY = { 235f - 34f, 235f, 235f + 34f, 235f - 68f, 235f + 68f };
            for (int i = 0; i < noteX.Length; i++)
            {
                SheetTheme.Note(root, new Vector2(noteX[i], noteY[i]), 26f, SheetTheme.Ink);
            }

            SheetTheme.Fill(root, "Rule", new Vector2(760f, 2f), new Vector2(0f, 130f), new Color(0.15f, 0.13f, 0.11f, 0.25f));

            // 按钮
            UIWidgets.Button(root, "开 始 游 戏", new Vector2(460f, 92f), new Vector2(0f, 20f), StartGame,
                SheetTheme.Ink, SheetTheme.Parchment, 32);
            UIWidgets.Button(root, "牌 组", new Vector2(300f, 68f), new Vector2(0f, -90f), OpenDeck,
                SheetTheme.ParchmentDark, SheetTheme.Ink, 26);
            UIWidgets.Button(root, "设 置", new Vector2(300f, 68f), new Vector2(0f, -178f), OpenSettings,
                SheetTheme.ParchmentDark, SheetTheme.Ink, 26);
            UIWidgets.Button(root, "退 出", new Vector2(300f, 68f), new Vector2(0f, -266f), Quit,
                new Color(0.86f, 0.80f, 0.67f, 0.6f), SheetTheme.Ink, 26);

            UIWidgets.Label(root, "Space 开始 · B 牌组 · O 设置", 22, TextAnchor.MiddleCenter, SheetTheme.InkSoft,
                new Vector2(900f, 40f), new Vector2(0f, -380f));
            UIWidgets.Label(root, "v0.4  prototype", 20, TextAnchor.MiddleRight, SheetTheme.InkSoft,
                new Vector2(400f, 36f), new Vector2(720f, -500f));
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

        private void HideTemplateChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
