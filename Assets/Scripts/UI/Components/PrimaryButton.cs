using ProDomino.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProDomino.UI.Components
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(UIGradient))]
    [RequireComponent(typeof(LayoutElement))]
    public class PrimaryButton : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string labelText = "Button";

        private Button button;
        private Image background;
        private UIGradient gradient;
        private LayoutElement layoutElement;

        public Button.ButtonClickedEvent OnClick => button != null ? button.onClick : null;

        public string LabelText
        {
            get => labelText;
            set
            {
                labelText = value;
                if (label != null)
                    label.text = labelText;
            }
        }

        public bool Interactable
        {
            get => button != null && button.interactable;
            set
            {
                if (button != null)
                    button.interactable = value;
            }
        }

        public void AddListener(UnityAction action)
        {
            button?.onClick.AddListener(action);
        }

        private void Awake()
        {
            Cache();
            EnsureLabel();
            ApplyLayout();
            ApplyTheme();
        }

        private void OnValidate()
        {
            Cache();
            EnsureLabel();
            ApplyLayout();
            ApplyTheme();
        }

        private void Cache()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            gradient = GetComponent<UIGradient>();
            layoutElement = GetComponent<LayoutElement>();
        }

        private void EnsureLabel()
        {
            if (label == null)
                label = GetComponentInChildren<TMP_Text>(true);

            if (label == null)
            {
                var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.transform.SetParent(transform, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                label = go.GetComponent<TextMeshProUGUI>();
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
            }

            label.text = labelText;
        }

        private void ApplyLayout()
        {
            float height = theme != null ? theme.primaryButtonHeight : 56f;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(1f, rect.anchorMax.y);
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
            layoutElement.flexibleWidth = 1f;
            layoutElement.flexibleHeight = 0f;
        }

        private void ApplyTheme()
        {
            if (theme == null)
                return;

            background.color = Color.white;
            gradient.GradientDirection = UIGradient.Direction.Horizontal;
            gradient.StartColor = theme.primaryButtonStart;
            gradient.EndColor = theme.primaryButtonEnd;

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
            button.colors = colors;

            theme.ApplyTo(label, UIFontWeight.Bold, theme.heading4Size, theme.primaryButtonLabel);
            if (label != null)
                label.text = labelText;
        }
    }
}
