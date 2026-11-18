using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>
    /// 界面开关动画。项目暂时没有引入 DOTween（Asset Store 无法在批处理里安装），
    /// 这里用协程实现等价的淡入/缩放/淡出；之后接入 DOTween 只需替换本文件的实现。
    /// </summary>
    public static class UIAnimator
    {
        public const float OpenDuration = 0.18f;
        public const float CloseDuration = 0.12f;
        private const float StartScale = 0.96f;

        public static void PlayOpen(MonoBehaviour host, RectTransform root)
        {
            if (host == null || root == null)
            {
                return;
            }

            CanvasGroup group = EnsureCanvasGroup(root);
            group.alpha = 0f;
            root.localScale = Vector3.one * StartScale;
            host.StartCoroutine(OpenRoutine(group, root));
        }

        public static IEnumerator PlayClose(MonoBehaviour host, RectTransform root)
        {
            if (host == null || root == null)
            {
                yield break;
            }

            CanvasGroup group = EnsureCanvasGroup(root);
            float time = 0f;
            float startAlpha = group.alpha;
            while (time < CloseDuration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / CloseDuration);
                group.alpha = Mathf.Lerp(startAlpha, 0f, t);
                root.localScale = Vector3.one * Mathf.Lerp(1f, StartScale, t);
                yield return null;
            }

            group.alpha = 0f;
        }

        private static IEnumerator OpenRoutine(CanvasGroup group, RectTransform root)
        {
            float time = 0f;
            while (time < OpenDuration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / OpenDuration);
                float eased = 1f - (1f - t) * (1f - t);
                group.alpha = eased;
                root.localScale = Vector3.one * Mathf.Lerp(StartScale, 1f, eased);
                yield return null;
            }

            group.alpha = 1f;
            root.localScale = Vector3.one;
        }

        private static CanvasGroup EnsureCanvasGroup(RectTransform root)
        {
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = root.gameObject.AddComponent<CanvasGroup>();
            }

            return group;
        }
    }
}
