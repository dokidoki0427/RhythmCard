using System;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.UI
{
    /// <summary>运行时搭 UI 的小工具（设置/牌组这种纯代码界面用）。</summary>
    public static class UIWidgets
    {
        public static RectTransform Panel(Transform parent, string name, Vector2 size, Vector2 position, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        public static Text Label(Transform parent, string content, int size, TextAnchor anchor,
            Color color, Vector2 boxSize, Vector2 position)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = boxSize;
            rect.anchoredPosition = position;

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            return text;
        }

        public static Button Button(Transform parent, string content, Vector2 size, Vector2 position,
            Action onClick, Color background)
        {
            var go = new GameObject("Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.AddComponent<Image>();
            image.color = background;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            var text = Label(go.transform, content, 26, TextAnchor.MiddleCenter, Color.white, size, Vector2.zero);
            text.raycastTarget = false;
            return button;
        }
    }
}
