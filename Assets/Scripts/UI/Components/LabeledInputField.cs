using ProDomino.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.UI.Components
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(VerticalLayoutGroup))]
    [RequireComponent(typeof(LayoutElement))]
    public class LabeledInputField : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private RectTransform trailingIconSlot;
        [SerializeField] private string labelText = "Label";
        [SerializeField] private string placeholderText = "Enter text";

        private VerticalLayoutGroup layoutGroup;
        private LayoutElement layoutElement;

        public TMP_InputField Input => inputField;
        public string Text
        {
            get => inputField != null ? inputField.text : string.Empty;
            set
            {
                if (inputField != null)
                    inputField.text = value;
            }
        }

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

        public string PlaceholderText
        {
            get => placeholderText;
            set
            {
                placeholderText = value;
                ApplyPlaceholder();
            }
        }

        public RectTransform TrailingIconSlot => trailingIconSlot;

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
            layoutGroup = GetComponent<VerticalLayoutGroup>();
            layoutElement = GetComponent<LayoutElement>();
        }

        private void EnsureHierarchy()
        {
            if (label == null)
            {
                Transform existing = transform.Find("Label");
                if (existing != null)
                    label = existing.GetComponent<TMP_Text>();
            }

            if (label == null)
            {
                var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
                go.transform.SetParent(transform, false);
                label = go.GetComponent<TextMeshProUGUI>();
                label.alignment = TextAlignmentOptions.MidlineLeft;
                var labelLayout = go.GetComponent<LayoutElement>();
                labelLayout.minHeight = 20f;
                labelLayout.preferredHeight = 20f;
                labelLayout.flexibleWidth = 1f;
            }

            label.text = labelText;

            if (inputField == null)
                inputField = GetComponentInChildren<TMP_InputField>(true);

            if (inputField == null)
                inputField = CreateInputField();

            if (trailingIconSlot == null)
            {
                Transform existing = inputField.transform.Find("TrailingIconSlot");
                if (existing != null)
                    trailingIconSlot = existing as RectTransform;
            }

            if (trailingIconSlot == null)
            {
                var go = new GameObject("TrailingIconSlot", typeof(RectTransform));
                go.transform.SetParent(inputField.transform, false);
                trailingIconSlot = go.GetComponent<RectTransform>();
                trailingIconSlot.anchorMin = new Vector2(1f, 0.5f);
                trailingIconSlot.anchorMax = new Vector2(1f, 0.5f);
                trailingIconSlot.pivot = new Vector2(1f, 0.5f);
                trailingIconSlot.sizeDelta = new Vector2(24f, 24f);
                trailingIconSlot.anchoredPosition = new Vector2(-20f, 0f);
                go.SetActive(false);
            }
        }

        private TMP_InputField CreateInputField()
        {
            var fieldGo = new GameObject("Field", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
            fieldGo.transform.SetParent(transform, false);

            var fieldLayout = fieldGo.GetComponent<LayoutElement>();
            fieldLayout.minHeight = theme != null ? theme.inputFieldHeight : 48f;
            fieldLayout.preferredHeight = fieldLayout.minHeight;
            fieldLayout.flexibleWidth = 1f;

            var viewportGo = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(fieldGo.transform, false);
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(20f, 4f);
            viewport.offsetMax = new Vector2(-20f, -4f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(viewportGo.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;

            var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            placeholderGo.transform.SetParent(viewportGo.transform, false);
            var placeholderRect = placeholderGo.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            var placeholder = placeholderGo.GetComponent<TextMeshProUGUI>();
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.textWrappingMode = TextWrappingModes.NoWrap;
            placeholder.raycastTarget = false;

            var field = fieldGo.GetComponent<TMP_InputField>();
            field.textViewport = viewport;
            field.textComponent = text;
            field.placeholder = placeholder;
            field.lineType = TMP_InputField.LineType.SingleLine;
            return field;
        }

        private void ApplyLayout()
        {
            float totalHeight = theme != null ? theme.labeledInputHeight : 80f;
            float fieldHeight = theme != null ? theme.inputFieldHeight : 48f;
            float spacing = theme != null ? theme.labeledInputSpacing : 12f;

            layoutGroup.spacing = spacing;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);

            layoutElement.minHeight = totalHeight;
            layoutElement.preferredHeight = totalHeight;
            layoutElement.flexibleWidth = 1f;
            layoutElement.flexibleHeight = 0f;

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(1f, rect.anchorMax.y);
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);

            if (inputField != null)
            {
                var fieldLayout = inputField.GetComponent<LayoutElement>();
                if (fieldLayout != null)
                {
                    fieldLayout.minHeight = fieldHeight;
                    fieldLayout.preferredHeight = fieldHeight;
                    fieldLayout.flexibleWidth = 1f;
                }
            }
        }

        private void ApplyTheme()
        {
            if (theme == null)
                return;

            theme.ApplyTo(label, UIFontWeight.Medium, theme.heading6Size, theme.black50);
            if (label != null)
                label.text = labelText;

            var fieldImage = inputField.GetComponent<Image>();
            if (fieldImage != null)
                fieldImage.color = theme.inputFill;

            theme.ApplyTo(inputField.textComponent, UIFontWeight.Regular, theme.bodySize, theme.secondaryColor);
            ApplyPlaceholder();
        }

        private void ApplyPlaceholder()
        {
            if (inputField == null || inputField.placeholder == null)
                return;

            if (inputField.placeholder is TMP_Text placeholder)
            {
                placeholder.text = placeholderText;
                if (theme != null)
                    theme.ApplyTo(placeholder, UIFontWeight.Regular, theme.bodySize, theme.black300);
            }
        }
    }
}
