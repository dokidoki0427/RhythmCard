using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CardRhythm.Rhythm
{
    /// <summary>
    /// 纯色占位战斗视图：
    ///   左：角色区（预留，正式美术替换）
    ///   中：4 轨音游区（判定线 + 音符 + 键位提示 + 判定文字）
    ///   右：怪物区（预留）
    /// 正式美术接入时整类替换，判定逻辑不受影响。
    /// </summary>
    public class NoteStageView : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float SidePanelWidth = 380f;
        private const float LaneAreaX = SidePanelWidth;
        private const float LaneAreaWidth = ReferenceWidth - SidePanelWidth * 2f;
        private const float LaneWidth = LaneAreaWidth / 4f;
        private const float JudgeLineY = 150f;
        private const float NoteWidth = 240f;
        private const float NoteHeight = 26f;
        private const float PopupLifetime = 0.55f;

        private readonly List<Image> m_NoteViews = new List<Image>();
        private readonly List<Image> m_KeyCaps = new List<Image>();
        private readonly List<Text> m_KeyLabels = new List<Text>();
        private readonly List<JudgementPopup> m_Popups = new List<JudgementPopup>();
        private readonly List<PlayedCardView> m_PlayedCards = new List<PlayedCardView>();
        private readonly Image[] m_LaneFlashes = new Image[4];
        private readonly Color[] m_LaneFlashColors = new Color[4];
        private readonly float[] m_LaneFlashAlphas = new float[4];

        private const int MaxPlayedCards = 5;
        private const float PlayedCardHeight = 92f;

        private RectTransform m_CharacterPanel;

        private Chart m_Chart;
        private Conductor m_Conductor;
        private Judger m_Judger;
        private Transform m_CanvasRoot;
        private float m_TravelSeconds = 1f;

        public static float LaneCenterX(int lane)
        {
            return LaneAreaX + (lane + 0.5f) * LaneWidth;
        }

        public void Build(Chart chart, Conductor conductor, IReadOnlyDictionary<string, Color> colors, Judger judger)
        {
            m_Chart = chart;
            m_Conductor = conductor;
            m_Judger = judger;
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
            m_CanvasRoot = canvasObject.transform;

            m_CharacterPanel = BuildSidePanel(canvasObject.transform, "CharacterPanel", "CHARACTER", 0f, new Color(0.14f, 0.2f, 0.3f, 0.9f));
            BuildSidePanel(canvasObject.transform, "MonsterPanel", "MONSTER", ReferenceWidth - SidePanelWidth, new Color(0.3f, 0.14f, 0.16f, 0.9f));

            for (int lane = 0; lane < 4; lane++)
            {
                var laneRect = CreateRect(canvasObject.transform, "Lane" + lane);
                laneRect.anchorMin = new Vector2(0f, 0f);
                laneRect.anchorMax = new Vector2(0f, 1f);
                laneRect.pivot = new Vector2(0f, 0f);
                laneRect.anchoredPosition = new Vector2(LaneAreaX + lane * LaneWidth, 0f);
                laneRect.sizeDelta = new Vector2(LaneWidth, 0f);
                var image = laneRect.gameObject.AddComponent<Image>();
                image.color = lane % 2 == 0 ? new Color(1f, 1f, 1f, 0.05f) : new Color(1f, 1f, 1f, 0.09f);
                image.raycastTarget = false;

                BuildKeyHint(laneRect, lane);
            }

            var judgeLine = CreateRect(canvasObject.transform, "JudgeLine");
            judgeLine.anchorMin = new Vector2(0f, 0f);
            judgeLine.anchorMax = new Vector2(0f, 0f);
            judgeLine.pivot = new Vector2(0f, 0.5f);
            judgeLine.sizeDelta = new Vector2(LaneAreaWidth, 4f);
            judgeLine.anchoredPosition = new Vector2(LaneAreaX, JudgeLineY);
            var lineImage = judgeLine.gameObject.AddComponent<Image>();
            lineImage.color = new Color(1f, 1f, 1f, 0.65f);
            lineImage.raycastTarget = false;

            for (int lane = 0; lane < 4; lane++)
            {
                var flashRect = CreateRect(canvasObject.transform, "LaneFlash" + lane);
                flashRect.anchorMin = new Vector2(0f, 0f);
                flashRect.anchorMax = new Vector2(0f, 0f);
                flashRect.pivot = new Vector2(0f, 0.5f);
                flashRect.sizeDelta = new Vector2(LaneWidth, 18f);
                flashRect.anchoredPosition = new Vector2(LaneAreaX + lane * LaneWidth, JudgeLineY);

                var flashImage = flashRect.gameObject.AddComponent<Image>();
                flashImage.color = new Color(1f, 1f, 1f, 0f);
                flashImage.raycastTarget = false;
                m_LaneFlashes[lane] = flashImage;
            }

            for (int i = 0; i < chart.Notes.Count; i++)
            {
                NoteData note = chart.Notes[i];
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

            BuildPopupPool();

            if (m_Judger != null)
            {
                m_Judger.JudgedEvent += OnJudged;
            }
        }

        private void OnDestroy()
        {
            if (m_Judger != null)
            {
                m_Judger.JudgedEvent -= OnJudged;
            }
        }

        private RectTransform BuildSidePanel(Transform parent, string name, string label, float x, Color color)
        {
            var panel = CreateRect(parent, name);
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 0f);
            panel.anchoredPosition = new Vector2(x, 0f);
            panel.sizeDelta = new Vector2(SidePanelWidth, 0f);

            var image = panel.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            var text = CreateText(panel, label, 40, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.55f));
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.sizeDelta = Vector2.zero;
            return panel;
        }

        /// <summary>在角色区追加一张“刚打出的牌”，按打出顺序从上往下排。</summary>
        public void ShowPlayedCard(string cardId, string displayName, Color accent, int damage)
        {
            if (m_CharacterPanel == null)
            {
                return;
            }

            var root = CreateRect(m_CharacterPanel, "Played_" + cardId);
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);
            root.sizeDelta = new Vector2(SidePanelWidth - 24f, PlayedCardHeight);

            var background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);
            background.raycastTarget = false;

            var stripe = CreateRect(root, "Accent");
            stripe.anchorMin = new Vector2(0f, 0f);
            stripe.anchorMax = new Vector2(0f, 1f);
            stripe.pivot = new Vector2(0f, 0.5f);
            stripe.sizeDelta = new Vector2(6f, 0f);
            stripe.anchoredPosition = Vector2.zero;
            var stripeImage = stripe.gameObject.AddComponent<Image>();
            stripeImage.color = accent;
            stripeImage.raycastTarget = false;

            var name = CreateText(root, displayName, 28, TextAnchor.MiddleLeft, Color.white);
            name.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            name.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            name.rectTransform.pivot = new Vector2(0f, 0.5f);
            name.rectTransform.sizeDelta = new Vector2(-90f, 40f);
            name.rectTransform.anchoredPosition = new Vector2(18f, 12f);

            var damageLabel = CreateText(root, "+" + damage, 30, TextAnchor.MiddleRight, accent);
            damageLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            damageLabel.rectTransform.anchorMax = new Vector2(1f, 0f);
            damageLabel.rectTransform.pivot = new Vector2(1f, 0f);
            damageLabel.rectTransform.sizeDelta = new Vector2(-24f, 36f);
            damageLabel.rectTransform.anchoredPosition = new Vector2(-12f, 6f);

            m_PlayedCards.Insert(0, new PlayedCardView { Root = root });
            while (m_PlayedCards.Count > MaxPlayedCards)
            {
                PlayedCardView last = m_PlayedCards[m_PlayedCards.Count - 1];
                m_PlayedCards.RemoveAt(m_PlayedCards.Count - 1);
                if (last.Root != null)
                {
                    Destroy(last.Root.gameObject);
                }
            }

            RepositionPlayedCards();
        }

        /// <summary>判定成功时给对应轨的判定线打一道颜色。</summary>
        public void FlashLane(int lane, Color color)
        {
            if (lane < 0 || lane >= m_LaneFlashes.Length || m_LaneFlashes[lane] == null)
            {
                return;
            }

            m_LaneFlashColors[lane] = color;
            m_LaneFlashAlphas[lane] = 1f;
        }

        private void RepositionPlayedCards()
        {
            for (int i = 0; i < m_PlayedCards.Count; i++)
            {
                RectTransform root = m_PlayedCards[i].Root;
                if (root != null)
                {
                    root.anchoredPosition = new Vector2(12f, -16f - i * (PlayedCardHeight + 10f));
                }
            }
        }

        private void UpdateLaneFlashes()
        {
            for (int lane = 0; lane < m_LaneFlashes.Length; lane++)
            {
                Image image = m_LaneFlashes[lane];
                if (image == null)
                {
                    continue;
                }

                m_LaneFlashAlphas[lane] = Mathf.Max(0f, m_LaneFlashAlphas[lane] - Time.deltaTime * 4f);
                Color color = m_LaneFlashColors[lane];
                color.a = m_LaneFlashAlphas[lane] * 0.9f;
                image.color = color;
            }
        }

        private void BuildKeyHint(RectTransform laneRect, int lane)
        {
            var cap = CreateRect(laneRect, "KeyCap");
            cap.anchorMin = new Vector2(0.5f, 0f);
            cap.anchorMax = new Vector2(0.5f, 0f);
            cap.pivot = new Vector2(0.5f, 0.5f);
            cap.sizeDelta = new Vector2(76f, 76f);
            cap.anchoredPosition = new Vector2(0f, 76f);

            var capImage = cap.gameObject.AddComponent<Image>();
            capImage.color = new Color(1f, 1f, 1f, 0.16f);
            capImage.raycastTarget = false;
            m_KeyCaps.Add(capImage);

            var label = CreateText(cap, LaneKey(lane), 42, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.85f));
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;
            m_KeyLabels.Add(label);
        }

        private void BuildPopupPool()
        {
            for (int i = 0; i < 12; i++)
            {
                var text = CreateText(m_CanvasRoot, "Popup" + i, 44, TextAnchor.MiddleCenter, Color.white);
                text.rectTransform.anchorMin = new Vector2(0f, 0f);
                text.rectTransform.anchorMax = new Vector2(0f, 0f);
                text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                text.rectTransform.sizeDelta = new Vector2(240f, 60f);
                text.gameObject.SetActive(false);

                m_Popups.Add(new JudgementPopup
                {
                    Text = text,
                    Age = PopupLifetime,
                    Lane = 0,
                });
            }
        }

        private void OnJudged(NoteData note, Judgement judgement)
        {
            JudgementPopup popup = FindFreePopup();
            if (popup == null)
            {
                return;
            }

            popup.Age = 0f;
            popup.Lane = note.Lane;
            popup.Text.text = LabelOf(judgement);
            popup.Text.color = ColorOf(judgement);
            popup.Text.gameObject.SetActive(true);
            popup.Text.rectTransform.anchoredPosition = new Vector2(LaneCenterX(note.Lane), JudgeLineY + 56f);
        }

        private void Update()
        {
            UpdateKeyFeedback();
            UpdatePopups();
            UpdateLaneFlashes();

            if (m_Chart == null || m_Conductor == null || !m_Conductor.IsRunning)
            {
                return;
            }

            float songTime = m_Conductor.SongTime;
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
                    view.rectTransform.anchoredPosition = new Vector2(LaneCenterX(note.Lane), y);
                }
            }
        }

        private void UpdateKeyFeedback()
        {
            for (int lane = 0; lane < m_KeyCaps.Count; lane++)
            {
                bool held = Input.GetKey(KeyCodeOf(lane));
                Color color = held ? new Color(1f, 0.85f, 0.35f, 0.85f) : new Color(1f, 1f, 1f, 0.16f);
                m_KeyCaps[lane].color = color;
                m_KeyLabels[lane].color = held ? Color.black : new Color(1f, 1f, 1f, 0.85f);
            }
        }

        private void UpdatePopups()
        {
            foreach (JudgementPopup popup in m_Popups)
            {
                if (!popup.Text.gameObject.activeSelf)
                {
                    continue;
                }

                popup.Age += Time.deltaTime;
                if (popup.Age >= PopupLifetime)
                {
                    popup.Text.gameObject.SetActive(false);
                    continue;
                }

                float t = popup.Age / PopupLifetime;
                Color color = popup.Text.color;
                color.a = 1f - t;
                popup.Text.color = color;

                Vector2 position = popup.Text.rectTransform.anchoredPosition;
                position.y = JudgeLineY + 56f + t * 40f;
                popup.Text.rectTransform.anchoredPosition = position;
            }
        }

        private JudgementPopup FindFreePopup()
        {
            foreach (JudgementPopup popup in m_Popups)
            {
                if (!popup.Text.gameObject.activeSelf)
                {
                    return popup;
                }
            }

            return null;
        }

        private static string LabelOf(Judgement judgement)
        {
            switch (judgement)
            {
                case Judgement.Perfect: return "PERFECT";
                case Judgement.Good: return "GOOD";
                case Judgement.Bad: return "BAD";
                default: return "MISS";
            }
        }

        public static Color ColorOf(Judgement judgement)
        {
            switch (judgement)
            {
                case Judgement.Perfect: return new Color(1f, 0.84f, 0.3f, 1f);
                case Judgement.Good: return new Color(0.5f, 0.9f, 0.5f, 1f);
                case Judgement.Bad: return new Color(0.75f, 0.75f, 0.75f, 1f);
                default: return new Color(1f, 0.35f, 0.35f, 1f);
            }
        }

        private static KeyCode KeyCodeOf(int lane)
        {
            switch (lane)
            {
                case 0: return KeyCode.D;
                case 1: return KeyCode.F;
                case 2: return KeyCode.J;
                default: return KeyCode.K;
            }
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Text CreateText(Transform parent, string content, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            return text;
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

        private sealed class JudgementPopup
        {
            public Text Text;
            public float Age;
            public int Lane;
        }

        private sealed class PlayedCardView
        {
            public RectTransform Root;
        }
    }
}
