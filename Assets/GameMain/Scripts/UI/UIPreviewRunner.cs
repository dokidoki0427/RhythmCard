using UnityEngine;

namespace CardRhythm.UI
{
    /// <summary>
    /// UI 预览场景的启动器：一次性打开三层各一个占位界面，用来目视检查层级顺序。
    /// 只用于预览，不参与正式流程。
    /// </summary>
    public class UIPreviewRunner : MonoBehaviour
    {
        private void Start()
        {
            UIRouter.Open(UIFormId.Menu);      // Main
            UIRouter.Open(UIFormId.BattleHud); // Hud
            UIRouter.Open(UIFormId.Settings);  // Popup

            Debug.Log("[UI-PREVIEW] request Menu(Main) + BattleHud(Hud) + Settings(Popup).");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIRouter.Back();
            }
        }
    }
}
