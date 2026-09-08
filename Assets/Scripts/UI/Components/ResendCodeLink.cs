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
    public class ResendCodeLink : MonoBehaviour
    {
        [SerializeField] private UITheme theme;
        [SerializeField] private string linkText = "Resend code";
        [SerializeField] private float cooldownSeconds = 60f;

        private HorizontalLayoutGroup layoutGroup;
        private LayoutElement layoutElement;
        private Button linkButton;
        private TextMeshProUGUI label;
        private float cooldownRemaining;

        public UnityEvent OnResendClicked = new UnityEvent();

        public bool IsCoolingDown => cooldownRemaining > 0f;

        public void BeginCooldown()
        {
            cooldownRemaining = cooldownSeconds;
            UpdateLabel();
            linkButton.interactable = false;
        }

        public void ResetCooldown()
        {
            cooldownRemaining = 0f;
            UpdateLabel();
            linkButton.interactable = true;
        }

        private void Awake()
        {
            if (theme == null)
                theme = AuthScreenLayout.LoadTheme();

            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
            UpdateLabel();
        }

        private void OnValidate()
        {
            Cache();
            EnsureHierarchy();
            ApplyLayout();
            ApplyTheme();
            UpdateLabel();
        }

        private void Update()
        {
            if (cooldownRemaining <= 0f)
                return;

            cooldownRemaining -= Time.unscaledDeltaTime;
            if (cooldownRemaining <= 0f)
            {
                cooldownRemaining = 0f;
                linkButton.interactable = true;
            }

            UpdateLabel();
        }

        private void Cache()
        {
            layoutGroup = GetComponent<HorizontalLayoutGroup>();
            layoutElement = GetComponent<LayoutElement>();
        }

        private void EnsureHierarchy()
        {
            if (linkButton != null && label != null)
                return;

            AuthScreenLayout.ClearChildren(transform);

            layoutGroup = GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
                layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();

            var linkGo = new GameObject("Link", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(Button), typeof(LayoutElement));
            linkGo.transform.SetParent(transform, false);

            label = linkGo.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;

            linkButton = linkGo.GetComponent<Button>();
            linkButton.transition = Selectable.Transition.ColorTint;
            linkButton.onClick.AddListener(HandleResendClicked);

            linkGo.GetComponent<LayoutElement>().flexibleWidth = 0f;
        }

        private void ApplyLayout()
        {
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = 0f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            layoutElement.minHeight = 20f;
            layoutElement.preferredHeight = 20f;
            layoutElement.flexibleWidth = 1f;
        }

        private void ApplyTheme()
        {
            if (theme == null || label == null || linkButton == null)
                return;

            theme.ApplyTo(label, UIFontWeight.Medium, theme.bodySize, theme.primaryColor);

            var colors = linkButton.colors;
            colors.normalColor = theme.primaryColor;
            colors.highlightedColor = theme.yellow400;
            colors.pressedColor = theme.yellow600;
            colors.selectedColor = theme.primaryColor;
            colors.disabledColor = theme.black100;
            linkButton.colors = colors;

            UpdateLabel();
        }

        private void HandleResendClicked()
        {
            if (IsCoolingDown)
                return;

            OnResendClicked.Invoke();
            BeginCooldown();
        }

        private void UpdateLabel()
        {
            if (label == null)
                return;

            if (cooldownRemaining > 0f)
            {
                int seconds = Mathf.CeilToInt(cooldownRemaining);
                label.text = $"Resend code in {seconds}s";
            }
            else
            {
                label.text = linkText;
            }
        }
    }
}
