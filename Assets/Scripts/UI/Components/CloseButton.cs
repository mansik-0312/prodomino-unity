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
    public class CloseButton : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private TMP_Text icon;

        private Button button;
        private Image background;
        private LayoutElement layoutElement;

        public Button.ButtonClickedEvent OnClick => button != null ? button.onClick : null;

        public void AddListener(UnityAction action)
        {
            button?.onClick.AddListener(action);
        }

        private void Awake()
        {
            Cache();
            EnsureIcon();
            ApplyLayout();
            ApplyTheme();
        }

        private void OnValidate()
        {
            Cache();
            EnsureIcon();
            ApplyLayout();
            ApplyTheme();
        }

        private void Cache()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            layoutElement = GetComponent<LayoutElement>();
        }

        private void EnsureIcon()
        {
            if (icon == null)
                icon = GetComponentInChildren<TMP_Text>(true);

            if (icon == null)
            {
                var go = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.transform.SetParent(transform, false);
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                icon = go.GetComponent<TextMeshProUGUI>();
                icon.alignment = TextAlignmentOptions.Center;
                icon.raycastTarget = false;
            }

            icon.text = "\u00D7";
        }

        private void ApplyLayout()
        {
            float size = theme != null ? theme.closeButtonSize : 50f;
            var rect = (RectTransform)transform;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);

            layoutElement.minWidth = size;
            layoutElement.minHeight = size;
            layoutElement.preferredWidth = size;
            layoutElement.preferredHeight = size;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;
        }

        private void ApplyTheme()
        {
            if (theme == null)
                return;

            background.color = theme.closeButtonFill;
            theme.ApplyTo(icon, UIFontWeight.Medium, theme.heading4Size, theme.secondaryColor);
            if (icon != null)
                icon.text = "\u00D7";
        }
    }
}
