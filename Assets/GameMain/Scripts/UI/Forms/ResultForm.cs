using CardRhythm.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 结算界面。布局在预制体里（标题 / 分数 / 连击 / 判定分布 / 准确率 / 返回按钮），
    /// 脚本只负责按名字填文字。
    /// </summary>
    public sealed class ResultForm : UIFormBase
    {
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            var result = userData as BattleResult;
            BindContinue();

            if (result == null)
            {
                SetText("Title", "RESULT");
                return;
            }

            Text title = FindText("Title");
            if (title != null)
            {
                title.text = result.Victory ? "VICTORY" : "DEFEAT";
                title.color = result.Victory ? SheetTheme.Gold : new Color(0.9f, 0.35f, 0.35f);
            }

            SetText("Score", "SCORE  " + result.Score);
            SetText("Combo", "MAX COMBO  " + result.MaxCombo);
            SetText("Judgement", "PERFECT " + result.Perfect + "    GOOD " + result.Good
                + "    BAD " + result.Bad + "    MISS " + result.Miss);
            SetText("Accuracy", "ACCURACY  " + (result.Accuracy * 100f).ToString("F1") + "%");
        }

        private void BindContinue()
        {
            Transform child = transform.Find("ContinueButton");
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
            button.onClick.AddListener(Continue);
        }

        private void SetText(string childName, string value)
        {
            Text text = FindText(childName);
            if (text != null)
            {
                text.text = value;
            }
        }

        private Text FindText(string childName)
        {
            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<Text>() : null;
        }

        private static void Continue()
        {
            var procedure = CardRhythm.Procedure.GameProcedureBase.Current;
            if (procedure != null)
            {
                procedure.RequestAdvance();
            }
        }
    }
}
