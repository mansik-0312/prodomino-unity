using ProDomino.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProDomino.UI.Components
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    [RequireComponent(typeof(LayoutElement))]
    public class UICheckbox : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private Image box;
        [SerializeField] private Image checkmark;
        [SerializeField] private TMP_Text label;
        [SerializeField] private string labelText = "Label";
        [SerializeField] private bool isOn;

        private Toggle toggle;
        private LayoutElement layoutElement;

        public Toggle.ToggleEvent OnValueChanged => toggle != null ? toggle.onValueChanged : null;

        public bool IsOn
        {
            get => toggle != null ? toggle.isOn : isOn;
            set
            {
                isOn = value;
                if (toggle != null)
                    toggle.isOn = value;
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

        public void AddListener(UnityAction<bool> action)
        {
            toggle?.onValueChanged.AddListener(action);
        }

        private void Awake()
        {
            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
            toggle.isOn = isOn;
        }

        private void OnValidate()
        {
            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
            if (toggle != null)
                toggle.isOn = isOn;
        }

        private void Cache()
        {
            toggle = GetComponent<Toggle>();
            layoutElement = GetComponent<LayoutElement>();
        }

        private void EnsureHierarchy()
        {
            if (box == null)
            {
                Transform existing = transform.Find("Box");
                if (existing != null)
                    box = existing.GetComponent<Image>();
            }

            if (box == null)
            {
                var go = new GameObject("Box", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
                go.transform.SetParent(transform, false);
                box = go.GetComponent<Image>();
                var boxLayout = go.GetComponent<LayoutElement>();
                boxLayout.minWidth = 24f;
                boxLayout.minHeight = 24f;
                boxLayout.preferredWidth = 24f;
                boxLayout.preferredHeight = 24f;
                boxLayout.flexibleWidth = 0f;
            }

            if (checkmark == null)
            {
                Transform existing = box.transform.Find("Checkmark");
                if (existing != null)
                    checkmark = existing.GetComponent<Image>();
            }

            if (checkmark == null)
            {
                var go = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(box.transform, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.2f, 0.2f);
                rect.anchorMax = new Vector2(0.8f, 0.8f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                checkmark = go.GetComponent<Image>();
                checkmark.raycastTarget = false;
            }

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
                go.GetComponent<LayoutElement>().flexibleWidth = 1f;
            }

            label.text = labelText;
            toggle.graphic = checkmark;
            toggle.targetGraphic = box;
        }

        private void ApplyLayout()
        {
            float height = theme != null ? theme.checkboxHeight : 24f;
            var group = GetComponent<HorizontalLayoutGroup>();
            group.spacing = 12f;
            group.childAlignment = TextAnchor.MiddleLeft;
            group.childControlHeight = true;
            group.childControlWidth = true;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = false;

            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
            layoutElement.flexibleWidth = 1f;

            var boxLayout = box.GetComponent<LayoutElement>();
            if (boxLayout != null)
            {
                boxLayout.minWidth = height;
                boxLayout.minHeight = height;
                boxLayout.preferredWidth = height;
                boxLayout.preferredHeight = height;
            }
        }

        private void ApplyTheme()
        {
            if (theme == null)
                return;

            box.color = theme.checkboxFill;
            checkmark.color = theme.primaryColor;
            theme.ApplyTo(label, UIFontWeight.Medium, theme.heading6Size, theme.secondaryColor);
            if (label != null)
                label.text = labelText;
        }
    }
}
