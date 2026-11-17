using CardRhythm.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 结算界面：大标题 + 数据明细 + 返回按钮。
    /// </summary>
    public sealed class ResultForm : UIFormBase
    {
        private Button m_ContinueButton;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            var result = userData as BattleResult;
            var title = FindText("Title");
            var hint = FindText("Hint");
            EnsureContinueButton();

            if (result == null)
            {
                if (title != null)
                {
                    title.text = "RESULT";
                    title.fontSize = 64;
                }

                if (hint != null)
                {
                    hint.text = "no battle data";
                }

                return;
            }

            if (title != null)
            {
                title.text = result.Victory ? "VICTORY" : "DEFEAT";
                title.fontSize = 72;
                title.color = result.Victory ? SheetTheme.Gold : new Color(0.9f, 0.35f, 0.35f);
            }

            if (hint != null)
            {
                hint.fontSize = 30;
                hint.text = "SCORE  " + result.Score
                    + "\n\nMAX COMBO  " + result.MaxCombo
                    + "\n\nPERFECT " + result.Perfect + "    GOOD " + result.Good
                    + "    BAD " + result.Bad + "    MISS " + result.Miss
                    + "\n\nACCURACY  " + (result.Accuracy * 100f).ToString("F1") + "%";
            }
        }

        private void EnsureContinueButton()
        {
            if (m_ContinueButton != null)
            {
                return;
            }

            Transform parent = transform.Find("Content");
            if (parent == null)
            {
                parent = transform;
            }

            m_ContinueButton = UIWidgets.Button(parent, "返回地图", new Vector2(320f, 72f), new Vector2(0f, -260f),
                Continue, SheetTheme.Gold, SheetTheme.Ink, 28);
        }

        private static void Continue()
        {
            var procedure = CardRhythm.Procedure.GameProcedureBase.Current;
            if (procedure != null)
            {
                procedure.RequestAdvance();
            }
        }

        private Text FindText(string childName)
        {
            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<Text>() : null;
        }
    }
}
