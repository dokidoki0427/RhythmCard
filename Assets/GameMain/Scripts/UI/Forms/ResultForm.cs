using CardRhythm.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>结算界面：把 BattleResult 填到占位文本里。</summary>
    public sealed class ResultForm : UIFormBase
    {
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            var result = userData as BattleResult;
            var title = FindText("Title");
            var hint = FindText("Hint");

            if (result == null)
            {
                if (title != null)
                {
                    title.text = "RESULT";
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
                title.color = result.Victory ? new Color(1f, 0.84f, 0.3f) : new Color(1f, 0.4f, 0.4f);
            }

            if (hint != null)
            {
                hint.text = "Score " + result.Score
                    + "\nMax Combo " + result.MaxCombo
                    + "\nPerfect " + result.Perfect + "   Good " + result.Good
                    + "   Bad " + result.Bad + "   Miss " + result.Miss
                    + "\nAccuracy " + (result.Accuracy * 100f).ToString("F1") + "%";
            }
        }

        private Text FindText(string childName)
        {
            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<Text>() : null;
        }
    }
}
