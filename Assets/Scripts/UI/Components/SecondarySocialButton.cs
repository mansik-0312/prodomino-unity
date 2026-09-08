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
    [RequireComponent(typeof(LayoutElement))]
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class SecondarySocialButton : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string labelText = "Login";

        private Button button;
        private Image background;
        private LayoutElement layoutElement;
        private HorizontalLayoutGroup layoutGroup;

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

        public Sprite IconSprite
        {
            set
            {
                if (icon == null)
                    return;

                icon.sprite = value;
                icon.enabled = value != null;
            }
        }

        public void SetTheme(UITheme value)
        {
            theme = value;
            ApplyLayout();
            ApplyTheme();
        }

        public void AddListener(UnityAction action)
        {
            button?.onClick.AddListener(action);
        }

        private void Awake()
        {
            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
        }

        private void OnValidate()
        {
            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
        }

        private void Cache()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            layoutElement = GetComponent<LayoutElement>();
            layoutGroup = GetComponent<HorizontalLayoutGroup>();
        }

        private void EnsureHierarchy()
        {
            if (layoutGroup == null)
                layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();

            if (icon == null)
            {
                Transform existing = transform.Find("Icon");
                if (existing != null)
                    icon = existing.GetComponent<Image>();
            }

            if (icon == null)
            {
                var go = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
                go.transform.SetParent(transform, false);
                icon = go.GetComponent<Image>();
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                var iconLayout = go.GetComponent<LayoutElement>();
                iconLayout.minWidth = 20f;
                iconLayout.minHeight = 20f;
                iconLayout.preferredWidth = 20f;
                iconLayout.preferredHeight = 20f;
                iconLayout.flexibleWidth = 0f;
            }

            if (label == null)
                label = GetComponentInChildren<TMP_Text>(true);

            if (label == null)
            {
                var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
                go.transform.SetParent(transform, false);
                label = go.GetComponent<TextMeshProUGUI>();
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                go.GetComponent<LayoutElement>().flexibleWidth = 1f;
            }

            label.text = labelText;
        }

        private void ApplyLayout()
        {
            float height = theme != null ? theme.inputFieldHeight : 48f;

            layoutGroup.spacing = 16f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.padding = new RectOffset(12, 12, 12, 12);

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(1f, rect.anchorMax.y);
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

            background.color = theme.inputFill;

            var colors = button.colors;
            colors.normalColor = theme.inputFill;
            colors.highlightedColor = theme.black400;
            colors.pressedColor = theme.black300;
            colors.selectedColor = theme.inputFill;
            colors.disabledColor = new Color(theme.inputFill.r, theme.inputFill.g, theme.inputFill.b, 0.5f);
            button.colors = colors;

            theme.ApplyTo(label, UIFontWeight.Regular, theme.bodySize, theme.black100);
            if (label != null)
                label.text = labelText;
        }
    }
}
