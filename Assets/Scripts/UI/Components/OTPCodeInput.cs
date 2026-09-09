using ProDomino.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProDomino.UI.Components
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    [RequireComponent(typeof(LayoutElement))]
    public class OTPCodeInput : MonoBehaviour
    {
        private const int DefaultDigitCount = 4;
        private const float DefaultBoxSize = 70f;
        private const float DefaultBoxSpacing = 16f;

        [SerializeField] private UITheme theme;
        [SerializeField] private int digitCount = DefaultDigitCount;
        [SerializeField] private float boxSize = DefaultBoxSize;
        [SerializeField] private float boxSpacing = DefaultBoxSpacing;

        private HorizontalLayoutGroup layoutGroup;
        private LayoutElement layoutElement;
        private TMP_InputField hiddenInput;
        private TextMeshProUGUI[] digitLabels;
        private Image[] boxImages;

        public UnityEvent<string> OnValueChanged = new UnityEvent<string>();
        public UnityEvent<string> OnCodeComplete = new UnityEvent<string>();

        public string Code => hiddenInput != null ? hiddenInput.text : string.Empty;
        public bool IsComplete => Code.Length >= digitCount;

        public void Clear()
        {
            if (hiddenInput == null)
                return;

            hiddenInput.text = string.Empty;
            UpdateDigitDisplay();
        }

        public void Focus()
        {
            hiddenInput?.ActivateInputField();
        }

        private void Awake()
        {
            if (theme == null)
                theme = AuthScreenLayout.LoadTheme();

            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
            UpdateDigitDisplay();
        }

        private void OnValidate()
        {
            if (digitCount < 1)
                digitCount = DefaultDigitCount;

            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
            UpdateDigitDisplay();
        }

        private void Cache()
        {
            layoutGroup = GetComponent<HorizontalLayoutGroup>();
            layoutElement = GetComponent<LayoutElement>();
        }

        private void EnsureHierarchy()
        {
            if (digitLabels != null && digitLabels.Length == digitCount && hiddenInput != null)
                return;

            AuthScreenLayout.ClearChildren(transform);

            layoutGroup = GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
                layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();

            digitLabels = new TextMeshProUGUI[digitCount];
            boxImages = new Image[digitCount];

            for (int i = 0; i < digitCount; i++)
            {
                var boxGo = new GameObject($"Digit{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
                boxGo.transform.SetParent(transform, false);

                var boxLayout = boxGo.GetComponent<LayoutElement>();
                boxLayout.preferredWidth = boxSize;
                boxLayout.preferredHeight = boxSize;
                boxLayout.flexibleWidth = 0f;
                boxLayout.flexibleHeight = 0f;

                var boxImage = boxGo.GetComponent<Image>();
                boxImages[i] = boxImage;

                var digitGo = new GameObject("Digit", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                digitGo.transform.SetParent(boxGo.transform, false);
                AuthScreenLayout.StretchFull(digitGo.GetComponent<RectTransform>());

                var digitLabel = digitGo.GetComponent<TextMeshProUGUI>();
                digitLabel.alignment = TextAlignmentOptions.Center;
                digitLabel.textWrappingMode = TextWrappingModes.NoWrap;
                digitLabel.raycastTarget = false;
                digitLabels[i] = digitLabel;
            }

            hiddenInput = CreateHiddenInput();
        }

        private TMP_InputField CreateHiddenInput()
        {
            var inputGo = new GameObject("HiddenInput", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
            inputGo.transform.SetParent(transform, false);

            var inputLayout = inputGo.GetComponent<LayoutElement>();
            inputLayout.ignoreLayout = true;

            var inputRect = inputGo.GetComponent<RectTransform>();
            AuthScreenLayout.StretchFull(inputRect);

            var inputImage = inputGo.GetComponent<Image>();
            inputImage.color = new Color(1f, 1f, 1f, 0f);
            inputImage.raycastTarget = true;

            var viewportGo = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(inputGo.transform, false);
            var viewport = viewportGo.GetComponent<RectTransform>();
            AuthScreenLayout.StretchFull(viewport);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(viewportGo.transform, false);
            AuthScreenLayout.StretchFull(textGo.GetComponent<RectTransform>());
            var text = textGo.GetComponent<TextMeshProUGUI>();
            text.color = new Color(1f, 1f, 1f, 0f);

            var field = inputGo.GetComponent<TMP_InputField>();
            field.textViewport = viewport;
            field.textComponent = text;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.contentType = TMP_InputField.ContentType.IntegerNumber;
            field.characterLimit = digitCount;
            field.caretWidth = 0;
            field.customCaretColor = false;
            field.caretColor = new Color(0f, 0f, 0f, 0f);
            field.selectionColor = new Color(0f, 0f, 0f, 0f);
            field.onValueChanged.AddListener(OnHiddenInputChanged);
            field.onSelect.AddListener(_ => UpdateBoxFocus(true));
            field.onDeselect.AddListener(_ => UpdateBoxFocus(false));

            return field;
        }

        private void ApplyLayout()
        {
            layoutGroup.spacing = boxSpacing;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            float totalWidth = digitCount * boxSize + (digitCount - 1) * boxSpacing;
            layoutElement.minHeight = boxSize;
            layoutElement.preferredHeight = boxSize;
            layoutElement.preferredWidth = totalWidth;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(0.5f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(0.5f, rect.anchorMax.y);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(totalWidth, boxSize);
        }

        private void ApplyTheme()
        {
            if (theme == null)
                return;

            for (int i = 0; i < digitLabels.Length; i++)
            {
                theme.ApplyTo(digitLabels[i], UIFontWeight.SemiBold, theme.heading4Size, theme.secondaryColor);
                if (boxImages[i] != null)
                {
                    boxImages[i].color = theme.inputFill;
                }
            }

            if (hiddenInput != null)
                hiddenInput.characterLimit = digitCount;
        }

        private void OnHiddenInputChanged(string value)
        {
            if (hiddenInput == null)
                return;

            string digitsOnly = string.Empty;
            foreach (char c in value)
            {
                if (char.IsDigit(c))
                    digitsOnly += c;
            }

            if (digitsOnly != value)
            {
                hiddenInput.SetTextWithoutNotify(digitsOnly);
                hiddenInput.stringPosition = digitsOnly.Length;
            }

            UpdateDigitDisplay();

            OnValueChanged.Invoke(Code);
            if (IsComplete)
                OnCodeComplete.Invoke(Code);
        }

        private void UpdateDigitDisplay()
        {
            if (digitLabels == null)
                return;

            string code = Code;
            for (int i = 0; i < digitLabels.Length; i++)
            {
                digitLabels[i].text = i < code.Length ? code[i].ToString() : string.Empty;
            }
        }

        private void UpdateBoxFocus(bool focused)
        {
            if (theme == null || boxImages == null)
                return;

            for (int i = 0; i < boxImages.Length; i++)
            {
                bool activeDigit = focused && i == Mathf.Clamp(Code.Length, 0, digitCount - 1);
                boxImages[i].color = activeDigit ? theme.inputBorder : theme.inputFill;
            }
        }
    }
}
