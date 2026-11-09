using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.Rhythm
{
    /// <summary>
    /// 纯色占位视图：4 轨 + 判定线 + 音符块。
    /// 音符提前 2 拍出现，判定线在屏幕高度 15% 处，颜色取自卡牌强调色。
    /// 正式美术接入时整类替换，判定逻辑不受影响。
    /// </summary>
    public class NoteStageView : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float JudgeLineY = 162f;
        private const float NoteWidth = 360f;
        private const float NoteHeight = 28f;

        private readonly List<Image> m_NoteViews = new List<Image>();
        private Chart m_Chart;
        private Conductor m_Conductor;
        private float m_TravelSeconds = 1f;

        public void Build(Chart chart, Conductor conductor, IReadOnlyDictionary<string, Color> colors)
        {
            m_Chart = chart;
            m_Conductor = conductor;
            m_TravelSeconds = conductor.SecondsPerBeat * 2f;

            var canvasObject = new GameObject("NoteStage", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 260;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            float laneWidth = ReferenceWidth / 4f;
            for (int lane = 0; lane < 4; lane++)
            {
                var laneRect = CreateRect(canvasObject.transform, "Lane" + lane);
                laneRect.anchorMin = new Vector2(0f, 0f);
                laneRect.anchorMax = new Vector2(0f, 1f);
                laneRect.pivot = new Vector2(0f, 0f);
                laneRect.anchoredPosition = new Vector2(lane * laneWidth, 0f);
                laneRect.sizeDelta = new Vector2(laneWidth, 0f);
                var image = laneRect.gameObject.AddComponent<Image>();
                image.color = lane % 2 == 0 ? new Color(1f, 1f, 1f, 0.05f) : new Color(1f, 1f, 1f, 0.09f);
                image.raycastTarget = false;

                var label = CreateRect(laneRect, "Key").gameObject.AddComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 28;
                label.alignment = TextAnchor.LowerCenter;
                label.color = new Color(1f, 1f, 1f, 0.5f);
                label.text = LaneKey(lane);
                var labelRect = label.rectTransform;
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 0f);
                labelRect.pivot = new Vector2(0.5f, 0f);
                labelRect.sizeDelta = new Vector2(0f, 60f);
                labelRect.anchoredPosition = new Vector2(0f, -70f);
            }

            var judgeLine = CreateRect(canvasObject.transform, "JudgeLine");
            judgeLine.anchorMin = new Vector2(0f, 0f);
            judgeLine.anchorMax = new Vector2(1f, 0f);
            judgeLine.pivot = new Vector2(0.5f, 0.5f);
            judgeLine.sizeDelta = new Vector2(0f, 4f);
            judgeLine.anchoredPosition = new Vector2(0f, JudgeLineY);
            var lineImage = judgeLine.gameObject.AddComponent<Image>();
            lineImage.color = new Color(1f, 1f, 1f, 0.6f);
            lineImage.raycastTarget = false;

            foreach (NoteData note in chart.Notes)
            {
                var noteRect = CreateRect(canvasObject.transform, "Note_" + note.SourceCardId);
                noteRect.anchorMin = new Vector2(0f, 0f);
                noteRect.anchorMax = new Vector2(0f, 0f);
                noteRect.pivot = new Vector2(0.5f, 0.5f);
                noteRect.sizeDelta = new Vector2(NoteWidth, NoteHeight);

                var image = noteRect.gameObject.AddComponent<Image>();
                Color color;
                if (!colors.TryGetValue(note.SourceCardId, out color))
                {
                    color = Color.cyan;
                }

                color.a = 0.95f;
                image.color = color;
                image.raycastTarget = false;
                m_NoteViews.Add(image);
            }
        }

        private void Update()
        {
            if (m_Chart == null || m_Conductor == null || !m_Conductor.IsRunning)
            {
                return;
            }

            float songTime = m_Conductor.SongTime;
            float laneWidth = ReferenceWidth / 4f;

            for (int i = 0; i < m_Chart.Notes.Count; i++)
            {
                NoteData note = m_Chart.Notes[i];
                Image view = m_NoteViews[i];
                float remaining = note.Time - songTime;
                float y = JudgeLineY + remaining / m_TravelSeconds * (ReferenceHeight - JudgeLineY);
                bool visible = remaining > -0.35f && remaining < m_TravelSeconds + 0.5f;

                if (view.gameObject.activeSelf != visible)
                {
                    view.gameObject.SetActive(visible);
                }

                if (visible)
                {
                    view.rectTransform.anchoredPosition = new Vector2((note.Lane + 0.5f) * laneWidth, y);
                }
            }
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static string LaneKey(int lane)
        {
            switch (lane)
            {
                case 0: return "D";
                case 1: return "F";
                case 2: return "J";
                default: return "K";
            }
        }
    }
}
